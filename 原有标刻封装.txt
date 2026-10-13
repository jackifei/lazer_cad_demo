using System;
using System.Runtime.InteropServices;

namespace LazerClassLibrary
{
    /// <summary>
    /// MarkEzd.dll 的 P/Invoke 底层声明。
    /// 注意：MarkEzd.dll 为 32 位、cdecl 调用约定、字符串必须是 Unicode。
    /// </summary>
    internal static class NativeMethods
    {
        // 把 EZCAD 运行目录加入 DLL 搜索路径，确保能找到 MarkEzd.dll 及其依赖。
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_Initial2",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_Initial2(
            [MarshalAs(UnmanagedType.LPWStr)] string strEzCadPath,
            [MarshalAs(UnmanagedType.Bool)] bool bTestMode);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_Close",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_Close();

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_LoadEzdFile",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_LoadEzdFile(
            [MarshalAs(UnmanagedType.LPWStr)] string strFileName);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_ChangeTextByName",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_ChangeTextByName(
            [MarshalAs(UnmanagedType.LPWStr)] string strTextName,
            [MarshalAs(UnmanagedType.LPWStr)] string strTextNew);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_Mark",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_Mark(
            [MarshalAs(UnmanagedType.Bool)] bool bFlyMark);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_MarkEntity",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_MarkEntity(
            [MarshalAs(UnmanagedType.LPWStr)] string strEntName);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_StopMark",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lmc1_StopMark();

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_IsMarking",
            CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool lmc1_IsMarking();
    }
}
