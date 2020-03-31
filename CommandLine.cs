static class CommandLine {
    public static (int start, int length, int next, bool quot) ExtractArg(string commandLine, int start = 0) {
        var cmdLength = commandLine.Length;
        for(int i = start; i < cmdLength; i++) {
            var c = commandLine[i];
            if(c <= 0x20)
                continue;

            var i1 = i + 1;
            if(c == '"') {
                if(i1 < cmdLength) {
                    var ei = commandLine.IndexOf('"', i1);
                    return (ei > -1) ? (i1, ei - i1, ei + 1, true) : (i1, cmdLength - i1, cmdLength, true);
                }
                break;
            } else {
                if(i1 < cmdLength) {
                    var ei = commandLine.IndexOf(' ', i1);
                    return (ei > -1) ? (i, ei - i, ei, false) : (i, cmdLength - i, cmdLength, false);
                }
                return (i, 1, cmdLength, false);
            }
        }
        return (0, 0, 0, false);
    }

    public static (int start, int length) ExtractCommandName(string commandLine, int start, int length) {
        var ei = start + length - 1;
        var b = true;
        for(int i = ei; i >= start; i--) {
            var c = commandLine[i];
            if((c == '\\') || (c == '/') || (c == ':')) {
                return (i + 1, ei - i);
            } else if(b && (c == '.')) {
                b = false;
                ei = i - 1;
            }
        }
        return (start, ei - start + 1);
    }
}
