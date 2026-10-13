using System;
using System.IO;
using System.Threading.Tasks;

namespace LazerClassLibrary
{
    /// <summary>
    /// 面向 LabVIEW 的金橙子 MarkEzd.dll 高层封装。
    /// 只暴露日常打标流程：初始化、载入模板、改文本、打标、停止。
    /// </summary>
    public sealed class LaserMarkController : IDisposable
    {
        private readonly object _sync = new object();
        private volatile bool _initialized;
        private volatile int _lastError;
        private volatile Task _markTask;
        private volatile int _markResult;

        /// <summary>最近一次操作返回的错误码，0 表示成功。</summary>
        public int LastError
        {
            get { return _lastError; }
        }

        /// <summary>是否已经成功初始化。</summary>
        public bool IsInitialized
        {
            get { return _initialized; }
        }

        public LaserMarkController()
        {
        }

        /// <summary>
        /// 初始化 LMC1 控制卡。必须在其它方法之前调用。
        /// 内部使用 lmc1_Initial2，适合 LabVIEW 这类有桌面环境的图形程序。
        /// </summary>
        /// <param name="ezcadDirectory">ezcad2.exe 所在目录的完整路径（即整套 EZCAD 运行目录）。</param>
        /// <param name="testMode">测试模式：true 时不需要真实控制卡即可跑通接口流程。</param>
        /// <returns>0 成功，其它为错误码。</returns>
        public int Initialize(string ezcadDirectory, bool testMode)
        {
            if (_initialized)
            {
                return (int)Lmc1Error.Success;
            }

            string dir = PrepareDirectory(ezcadDirectory);
            if (dir == null)
            {
                return _lastError;
            }

            TrySetDllDirectory(dir);

            int rc = NativeMethods.lmc1_Initial2(dir, testMode);
            if (rc == (int)Lmc1Error.Success)
            {
                _initialized = true;
            }
            return SetError(rc);
        }

        /// <summary>关闭 LMC1 控制卡并释放资源。</summary>
        public int Close()
        {
            if (!_initialized)
            {
                return SetError((int)Lmc1Error.NoInitial);
            }

            int rc = NativeMethods.lmc1_Close();
            _initialized = false;
            return SetError(rc);
        }

        /// <summary>载入 ezd 模板文件，并清空当前数据库对象。</summary>
        public int LoadEzdFile(string filePath)
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            if (string.IsNullOrWhiteSpace(filePath)) return SetError((int)Lmc1Error.Param1);

            return SetError(NativeMethods.lmc1_LoadEzdFile(filePath));
        }

        /// <summary>修改指定名称文本对象的内容（通常在打标前调用）。</summary>
        public int ChangeTextByName(string entityName, string newText)
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            if (string.IsNullOrWhiteSpace(entityName)) return SetError((int)Lmc1Error.Param1);
            if (newText == null) newText = string.Empty;

            return SetError(NativeMethods.lmc1_ChangeTextByName(entityName, newText));
        }

        /// <summary>
        /// 标刻当前数据库里的所有对象。
        /// 注意：该方法是同步阻塞的，直到加工完成或用户停止才返回。
        /// </summary>
        public int Mark(bool flyMark)
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            return SetError(NativeMethods.lmc1_Mark(flyMark));
        }

        /// <summary>标刻当前数据库里指定名称的对象（同步阻塞）。</summary>
        public int MarkEntity(string entityName)
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            if (string.IsNullOrWhiteSpace(entityName)) return SetError((int)Lmc1Error.Param1);

            return SetError(NativeMethods.lmc1_MarkEntity(entityName));
        }

        /// <summary>
        /// 在后台线程启动打标，不阻塞调用方。配合 StopMark() 和 IsMarking() 使用。
        /// </summary>
        public int StartMark(bool flyMark)
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);

            lock (_sync)
            {
                if (_markTask != null && !_markTask.IsCompleted)
                {
                    return SetError((int)Lmc1Error.Status);
                }

                _markTask = Task.Run(() =>
                {
                    _markResult = NativeMethods.lmc1_Mark(flyMark);
                });
            }

            return SetError((int)Lmc1Error.Success);
        }

        /// <summary>判断控制卡是否正在打标。</summary>
        public bool IsMarking()
        {
            if (!_initialized) return false;
            return NativeMethods.lmc1_IsMarking();
        }

        /// <summary>强制停止当前打标或红光。</summary>
        public int StopMark()
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            return SetError(NativeMethods.lmc1_StopMark());
        }

        /// <summary>
        /// 读取 LMC1 的输入端口状态。
        /// data 的 Bit0 对应 IN0、Bit1 对应 IN1，以此类推；1 表示高电平。
        /// </summary>
        public int ReadPort(out ushort data)
        {
            data = 0;
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            return SetError(NativeMethods.lmc1_ReadPort(out data));
        }

        /// <summary>
        /// 写 LMC1 的输出端口。
        /// data 的 Bit0 对应 OUT0、Bit1 对应 OUT1，以此类推；1 表示输出高电平。
        /// </summary>
        public int WritePort(ushort data)
        {
            if (!_initialized) return SetError((int)Lmc1Error.NoInitial);
            return SetError(NativeMethods.lmc1_WritePort(data));
        }

        /// <summary>等待后台打标完成，返回本次打标的错误码。</summary>
        public int WaitForMarkCompleted()
        {
            Task task = _markTask;
            if (task == null)
            {
                return (int)Lmc1Error.Success;
            }

            task.Wait();
            return _markResult;
        }

        /// <summary>把错误码转换成可读的中文说明。</summary>
        public static string GetErrorMessage(int code)
        {
            return Lmc1ErrorText.GetMessage(code);
        }

        public void Dispose()
        {
            if (_initialized)
            {
                try
                {
                    NativeMethods.lmc1_Close();
                }
                catch (Exception)
                {
                    // 关闭失败时忽略，避免影响释放流程。
                }
                _initialized = false;
            }
        }

        private int SetError(int code)
        {
            _lastError = code;
            return code;
        }

        private string PrepareDirectory(string ezcadDirectory)
        {
            if (string.IsNullOrWhiteSpace(ezcadDirectory))
            {
                SetError((int)Lmc1Error.Param1);
                return null;
            }

            try
            {
                string dir = Path.GetFullPath(ezcadDirectory);
                if (!Directory.Exists(dir))
                {
                    SetError((int)Lmc1Error.NoFindCfgFile);
                    return null;
                }
                return dir;
            }
            catch (Exception)
            {
                SetError((int)Lmc1Error.Param1);
                return null;
            }
        }

        private void TrySetDllDirectory(string dir)
        {
            // 关键：把 EZCAD 运行目录加入 DLL 搜索路径，保证能找到 MarkEzd.dll 及 Lmc1.dll 等依赖。
            try
            {
                NativeMethods.SetDllDirectory(dir);
            }
            catch (Exception)
            {
                // 即使设置失败也继续尝试初始化，由 lmc1_Initial 系列返回具体错误码。
            }
        }
    }
}
