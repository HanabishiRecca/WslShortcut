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

    public unsafe static void ProcessOutput(IntPtr input, IntPtr output) {
        // Dangerous zone
        var buffer = stackalloc byte[Program.BufferSize];

        // First read is explicit to detect possible path output
        if(!(Win32.ReadFile(input, buffer, Program.BufferSize, out var count, null) && (count > 0)))
            return;

        var mntLen = mntPath.Length;
        var checkLen = mntLen + 1;
        char drive;

        // Detect plain path
        if((count >= checkLen) && CheckMntPath(buffer) && CheckDriveLetter(drive = (char)buffer[mntLen], false, true)) {
            // Very weird way to write 2 bytes
            short s;
            var sp = (byte*)&s;
            sp[0] = (byte)ToUpper(drive);
            sp[1] = (byte)':';
            Win32.WriteFile(output, sp, 2, null, null);

            if(count > checkLen) {
                // Seek for EOL and replace slashes along the way
                byte b;
                for(int i = checkLen; (i < count) && ((b = buffer[i]) != '\n'); i++)
                    if(b == '/')
                        buffer[i] = (byte)'\\';

                // Write remaining path
                Win32.WriteFile(output, buffer + checkLen, count - checkLen, null, null);
            }
        } else {
            Win32.WriteFile(output, buffer, count, null, null);
        }

        // Pump all the rest output
        while(Win32.ReadFile(input, buffer, Program.BufferSize, out count, null) && (count > 0))
            Win32.WriteFile(output, buffer, count, null, null);
    }

    unsafe static bool CheckMntPath(byte* p) {
        for(int i = 0; i < mntPath.Length; i++)
            if(p[i] != mntPath[i])
                return false;
        return true;
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
