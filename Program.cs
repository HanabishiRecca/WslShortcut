using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        var drive = str[0];
        if(!CheckDriveLetter(drive, true, true))
            return false;

        if(str.Length > 2) {
            // Absolute path
            var chr = str[2];
            if(!((chr == '\\') || (chr == '/')))
                return false;

            builder.Append('\'');

            builder.Append(mntPath);
            builder.Append(ToLower(drive));

            var len = builder.Length;
            var count = str.Length - 2;
            builder.Append(str, 2, count);
            builder.Replace('\\', '/', len, count);

            builder.Append('\'');
        } else {
            // Drive only
            builder.Append(mntPath);
            builder.Append(ToLower(drive));
        }

        return true;
    }

    static string PathFromWsl(string str) {
        // Detect WSL mounted path
        if(!((str.Length > mntPath.Length) && str.StartsWith(mntPath)))
            return str;

        var pathIndex = mntPath.Length + 1;
        if((str.Length > pathIndex) && (str[pathIndex] != '/'))
            return str;

        var drive = str[mntPath.Length];
        if(!CheckDriveLetter(drive, false, true))
            return str;

        // Convert WSL path
        var builder = new StringBuilder(str.Length);
        builder.Append(ToUpper(drive));
        builder.Append(':');

        if(str.Length > pathIndex) {
            var len = builder.Length;
            var count = str.Length - pathIndex;
            builder.Append(str, pathIndex, count);
            builder.Replace('/', '\\', len, count);
        }

        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool CheckDriveLetter(char chr, bool upper, bool lower) =>
        (upper && (chr >= 'A') && (chr <= 'Z')) ||
        (lower && (chr >= 'a') && (chr <= 'z'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static char ToUpper(char chr) => (char)(chr & -33);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static char ToLower(char chr) => (char)(chr | 32);
}
