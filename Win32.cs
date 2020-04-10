using System;
using System.Runtime.InteropServices;

static class Win32 {
    const string kernel32 = "kernel32.dll";

    [DllImport(kernel32, SetLastError = true)]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport(kernel32, SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport(kernel32, SetLastError = true)]
    public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);

    [DllImport(kernel32, SetLastError = true)]
    public static extern bool SetHandleInformation(IntPtr hObject, int dwMask, int dwFlags);

    [DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
    public unsafe static extern bool CreateProcessW(string lpApplicationName, string lpCommandLine, void* lpProcessAttributes, void* lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, void* lpEnvironment, string lpCurrentDirectory, STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [DllImport(kernel32, SetLastError = true)]
    public unsafe static extern bool ReadFile(IntPtr hFile, byte* lpBuffer, int nNumberOfBytesToRead, out int lpNumberOfBytesRead, void* lpOverlapped);

    [DllImport(kernel32, SetLastError = true)]
    public unsafe static extern bool WriteFile(IntPtr hFile, byte* lpBuffer, int nNumberOfBytesToWrite, int* lpNumberOfBytesWritten, void* lpOverlapped);

    [DllImport(kernel32, SetLastError = true)]
    public static extern bool SetConsoleCP(uint wCodePageID);

    [DllImport(kernel32, SetLastError = true)]
    public static extern bool SetConsoleOutputCP(uint wCodePageID);

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO {
        public int cb;
        public IntPtr lpReserved, lpDesktop, lpTitle;
        public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
        public short wShowWindow, cbReserved2;
        public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION {
        public IntPtr hProcess, hThread;
        public int dwProcessId, dwThreadId;
    }
}
