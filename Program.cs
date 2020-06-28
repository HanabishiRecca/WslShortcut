using System;
using System.Text;
using static Win32;

static class Program {
    // Set buffer to 512K as temporal workaround of https://github.com/microsoft/WSL/issues/5063
    public const int BufferSize = 512 * 1024;

    static void Main() {
        SetConsoleCP(65001);
        SetConsoleOutputCP(65001);

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
        while((arg = CommandLine.ExtractArg(cmd, arg.next)).next > 0) {
            builder.Append(' ');

            var quot = arg.quot || HasSpec(cmd, arg.start, arg.length);
            if(quot)
                builder.Append('"');

            if(!WslPath.PathToWsl(cmd, arg.start, arg.length, builder))
                builder.Append(cmd, arg.start, arg.length);

            if(quot)
                builder.Append('"');
        }

        PROCESS_INFORMATION proc;

        // Check for interactive mode
        if(Console.IsOutputRedirected) {
            // Create pipe
            var pipe = OpenPipe();
            if(!pipe.ok)
                throw new Exception("Failed to create pipe.");

            // Do not inherit reading end of pipe
            if(!SetHandleInformation(pipe.hRead, 1, 0))
                throw new Exception("Failed to set up pipe.");

            // Run WSL command
            proc = CreateProcess(builder.ToString(), pipe.hWrite);

            // Close own writing end of pipe to avoid ReadFile self locking
            if(!CloseHandle(pipe.hWrite))
                throw new Exception("Failed to close own writing handle.");

            WslPath.ProcessOutput(pipe.hRead, GetStdHandle(-11));
        } else {
            // Run WSL command without processing
            proc = CreateProcess(builder.ToString(), IntPtr.Zero);
        }

        WaitForSingleObject(proc.hProcess, -1);

        GetExitCodeProcess(proc.hProcess, out var exitCode);
        Environment.ExitCode = exitCode;
    }

    public unsafe static (bool ok, IntPtr hRead, IntPtr hWrite) OpenPipe() {
        var secAttrs = new SECURITY_ATTRIBUTES {
            nLength = sizeof(SECURITY_ATTRIBUTES),
            bInheritHandle = true,
        };
        return (CreatePipe(out var hReadPipe, out var hWritePipe, secAttrs, BufferSize), hReadPipe, hWritePipe);
    }

    public unsafe static PROCESS_INFORMATION CreateProcess(string commandLine, IntPtr hWritePipe) {
        var startupInfo = (hWritePipe == IntPtr.Zero) ? new STARTUPINFO() :
        new STARTUPINFO {
            cb = sizeof(STARTUPINFO),
            hStdInput = GetStdHandle(-10),
            hStdOutput = hWritePipe,
            hStdError = GetStdHandle(-12),
            dwFlags = 0x100,
        };

        if(!CreateProcessW(null, commandLine, null, null, true, 0, null, null, startupInfo, out var procInfo))
            throw new Exception("Failed to launch WSL.");

        return procInfo;
    }

    static bool HasSpec(string str, int index, int length) {
        var end = index + length;
        for(int i = index; i < end; i++)
            if(str[i] < 0x20)
                return true;
        return false;
    }
}
