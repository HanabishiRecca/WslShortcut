using System;
using System.Text;

static class Program {
    // Set buffer to 512K as temporal workaround of https://github.com/microsoft/WSL/issues/5063
    public const int BufferSize = 512 * 1024;

    static void Main() {
        Win32.SetConsoleCP(65001);
        Win32.SetConsoleOutputCP(65001);

        var cmd = Environment.CommandLine;

        // Extract command name
        var arg = CommandLine.ExtractArg(cmd);
        if(!(arg.length > 0))
            return;

        var command = CommandLine.ExtractCommandName(cmd, arg.start, arg.length);
        if(!(command.length > 0))
            return;

        var builder = new StringBuilder(cmd.Length);
        builder.Append("wsl.exe ");
        builder.Append(cmd, command.start, command.length);

        // Extract arguments and convert paths
        while((arg = CommandLine.ExtractArg(cmd, arg.next)).length > 0) {
            builder.Append(' ');

            if(arg.quot)
                builder.Append('"');

            if(!WslPath.PathToWsl(cmd, arg.start, arg.length, builder))
                builder.Append(cmd, arg.start, arg.length);

            if(arg.quot)
                builder.Append('"');
        }

        // Create pipe
        var pipe = CreatePipe();
        if(!pipe.ok)
            throw new Exception("Failed to create pipe.");

        // Do not inherit reading end of pipe
        if(!Win32.SetHandleInformation(pipe.hRead, 1, 0))
            throw new Exception("Failed to set up pipe.");

        // Run WSL command
        if(!CreateProcess(builder.ToString(), pipe.hWrite))
            throw new Exception("Failed to launch WSL.");

        // Close own writing end of pipe to avoid ReadFile self locking
        if(!Win32.CloseHandle(pipe.hWrite))
            throw new Exception("Failed to close own writing handle.");

        WslPath.ProcessOutput(pipe.hRead, Win32.GetStdHandle(-11));
    }

    public unsafe static (bool ok, IntPtr hRead, IntPtr hWrite) CreatePipe() {
        var secAttrs = new Win32.SECURITY_ATTRIBUTES {
            nLength = sizeof(Win32.SECURITY_ATTRIBUTES),
            bInheritHandle = true,
        };
        return (Win32.CreatePipe(out var hReadPipe, out var hWritePipe, secAttrs, BufferSize), hReadPipe, hWritePipe);
    }

    public unsafe static bool CreateProcess(string commandLine, IntPtr hWritePipe) {
        var startupInfo = new Win32.STARTUPINFO {
            cb = sizeof(Win32.STARTUPINFO),
            hStdInput = Win32.GetStdHandle(-10),
            hStdOutput = hWritePipe,
            hStdError = hWritePipe,
            dwFlags = 0x100,
        };

        var result = Win32.CreateProcessW(null, commandLine, null, null, true, 0, null, null, startupInfo, out var procInfo);
        if(result) {
            Win32.CloseHandle(procInfo.hThread);
            Win32.CloseHandle(procInfo.hProcess);
        }

        return result;
    }
}
