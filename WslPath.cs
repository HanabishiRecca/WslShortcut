using System;
using System.Runtime.CompilerServices;
using System.Text;

static class WslPath {
    const string mntPath = "/mnt/";

    public static bool PathToWsl(string str, int startIndex, int charCount, StringBuilder builder) {
        // Detect absolute path
        if(charCount < 2)
            return false;

        if(str[startIndex + 1] != ':')
            return false;

        var drive = str[startIndex];
        if(!CheckDriveLetter(drive, true, true))
            return false;

        var count = charCount - 2;
        if(count > 0) {
            // Absolute path
            var index = startIndex + 2;
            var chr = str[index];
            if(!((chr == '\\') || (chr == '/')))
                return false;

            builder.Append(mntPath);
            builder.Append(ToLower(drive));

            var len = builder.Length;
            builder.Append(str, index, count);
            builder.Replace('\\', '/', len, count);
        } else {
            // Drive only
            builder.Append(mntPath);
            builder.Append(ToLower(drive));
        }

        return true;
    }

    public static string PathFromWsl(string str) {
        // Detect WSL mounted path
        if(!((str.Length > mntPath.Length) && str.StartsWith(mntPath, StringComparison.Ordinal)))
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
