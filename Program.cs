using System;
using System.Diagnostics;
using System.Text;

static class Program {
    static void Main() {
        var cmd = Environment.CommandLine;

        // Extract command name
        var arg = CommandLine.ExtractArg(cmd);
        if(!(arg.length > 0))
            return;

        var command = CommandLine.ExtractCommandName(cmd, arg.start, arg.length);
        if(!(command.length > 0))
            return;

        var builder = new StringBuilder(cmd.Length);
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

        // Run WSL command
        var p = Process.Start(new ProcessStartInfo {
            FileName = "wsl.exe",
            Arguments = builder.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });

        // Convert paths in output
        while(!p.StandardOutput.EndOfStream)
            Console.WriteLine(WslPath.PathFromWsl(p.StandardOutput.ReadLine()));
    }
}
