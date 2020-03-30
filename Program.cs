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
            if(!PathToWsl(str, builder))
                builder.Append(str, 0, str.Length);
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

    const string mntPath = "/mnt/";

    static bool PathToWsl(string str, StringBuilder builder) {
        // Detect absolute path
        if(str.Length < 2)
            return false;

        if(str[1] != ':')
            return false;

        var chr = str[0];
        if(!(/* A-Z */ (chr >= 65 && chr <= 90) || /* a-z */ (chr >= 97 && chr <= 122)))
            return false;

        if(str.Length == 2) {
            // Drive only
            builder.Append(mntPath);
            builder.Append(char.ToLowerInvariant(chr));
            return true;
        }

        chr = str[2];
        if((str.Length > 2) && (chr != '\\') && (chr != '/'))
            return false;

        // Absolute path
        builder.Append('\'');

        builder.Append(mntPath);
        builder.Append(char.ToLowerInvariant(chr));
        builder.Append('/');

        if(str.Length > 3) {
            var len = builder.Length;
            var count = str.Length - 3;
            builder.Append(str, 3, count);
            builder.Replace('\\', '/', len, count);
        }

        builder.Append('\'');

        return true;
    }

    static string PathFromWsl(string str) {
        if(!str.StartsWith(mntPath))
            return str;

        // Convert WSL path
        var builder = new StringBuilder(str.Length);
        builder.Append(char.ToUpperInvariant(str[mntPath.Length]));
        builder.Append(":\\");

        var pathIndex = mntPath.Length + 2;
        if(str.Length > pathIndex) {
            var len = builder.Length;
            var count = str.Length - pathIndex;
            builder.Append(str, pathIndex, count);
            builder.Replace('/', '\\', len, count);
        }

        return builder.ToString();
    }
}
