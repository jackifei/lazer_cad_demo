using System;

namespace LazerClassLibrary
{
    /// <summary>
    /// MarkEzd.dll 通用错误码（与《MarkEzddll 手册》保持一致）。
    /// </summary>
    public enum Lmc1Error
    {
        Success = 0,
        EzcadRunning = 1,
        NoFindCfgFile = 2,
        FailedOpen = 3,
        NoDevice = 4,
        HardwareVersion = 5,
        DeviceCfg = 6,
        StopSignal = 7,
        UserStop = 8,
        Unknown = 9,
        OutTime = 10,
        NoInitial = 11,
        ReadFile = 12,
        OwnerWndNull = 13,
        NoFindFont = 14,
        PenNo = 15,
        NotText = 16,
        SaveFile = 17,
        NoFindEntity = 18,
        Status = 19,
        Param1 = 20,
        Param2 = 21,
    }

    public static class Lmc1ErrorText
    {
        public static string GetMessage(int code)
        {
            switch (code)
            {
                case 0: return "成功";
                case 1: return "发现 EZCAD 正在运行";
                case 2: return "找不到 EZCAD.CFG";
                case 3: return "打开 LMC1 失败";
                case 4: return "没有有效的 LMC1 设备";
                case 5: return "LMC1 版本错误";
                case 6: return "找不到设备配置文件";
                case 7: return "报警信号";
                case 8: return "用户停止";
                case 9: return "不明错误";
                case 10: return "超时";
                case 11: return "未初始化";
                case 12: return "读文件错误";
                case 13: return "窗口为空";
                case 14: return "找不到指定名称的字体";
                case 15: return "错误的笔号";
                case 16: return "指定名称的对象不是文本对象";
                case 17: return "保存文件失败";
                case 18: return "找不到指定对象";
                case 19: return "当前状态下不能执行此操作";
                case 20: return "错误的执行参数";
                case 21: return "错误的硬件参数";
                default: return "未知错误码：" + code;
            }
        }
    }
}
