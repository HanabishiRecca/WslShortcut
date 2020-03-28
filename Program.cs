using System;
using System.Diagnostics;
using System.Text;

static class Program {
    static void Main(string[] args) {
        // Using current process name as a command name
        var builder = new StringBuilder(Process.GetCurrentProcess().ProcessName);

        // Build command line
        foreach(var str in args) {
            builder.Append(' ');
            PathToWsl(str, builder);
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
            Console.WriteLine(PathFromWsl(p.StandardOutput.ReadLine()));

        p.WaitForExit();
    }

    static void PathToWsl(string str, StringBuilder builder) {
        var index = str.IndexOf(@":\");
        if(index > 0) {
            // Convert absolute path
            var driveIndex = index - 1;
            builder.Append('\'');
            builder.Append(str, 0, driveIndex);
            builder.Append("/mnt/");
            builder.Append(char.ToLowerInvariant(str[driveIndex]));
            builder.Append('/');
            var pathIndex = index + 2;
            if(str.Length > pathIndex) {
                var len = builder.Length;
                var count = str.Length - pathIndex;
                builder.Append(str, pathIndex, count);
                builder.Replace('\\', '/', len, count);
            }
            builder.Append('\'');
        } else if(str.IndexOf('\\') > -1) {
            // Convert relative path
            builder.Append('\'');
            var len = builder.Length;
            builder.Append(str);
            builder.Replace('\\', '/', len, str.Length);
            builder.Append('\'');
        } else {
            // As is
            builder.Append(str, 0, str.Length);
        }
    }

    static string PathFromWsl(string str) {
        var index = str.IndexOf("/mnt/");
        if(index > -1) {
            // Convert absolute path
            var builder = new StringBuilder(str.Length);
            builder.Append(str, 0, index);
            builder.Append(char.ToUpperInvariant(str[index + 5]));
            builder.Append(@":\");
            var pathIndex = index + 7;
            if(str.Length > pathIndex) {
                var len = builder.Length;
                var count = str.Length - pathIndex;
                builder.Append(str, pathIndex, count);
                builder.Replace('/', '\\', len, count);
            }
            return builder.ToString();
        } else if(str.IndexOf('/') > -1) {
            // Convert relative path
            var builder = new StringBuilder();
            builder.Append(str);
            builder.Replace('/', '\\');
            return builder.ToString();
        } else {
            // As is
            return str;
        }
    }
}
