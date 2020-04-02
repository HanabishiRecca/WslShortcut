# WSL Shortcut

Run **WSL** commands directly in Windows.

Also allows to use **WSL** `git`/`node`/etc. in **Visual Studio Code** or another software.

Combines functionality of utilities like [`wslgit`](//github.com/andy-5/wslgit), [`wslnodejs`](//github.com/snooopcatt/wslnodejs), [`wslexec`](//github.com/int128/wslexec) etc. with simpler usage.

### [Releases](../../releases)

### Usage

- Rename `WslShortcut.exe` to desired command name, e.g. `git.exe`, `node.exe`, `ls.exe` etc. You can make a renamed copy for every command you want.
- Place this executable(s) to some **PATH** directory (`Windows`, `System32` or make your own).
- Now you can run it directly in **cmd** (e.g. `git status`, `node -v`, `ls -la`), **VS Code** should find `git`/`node` automatically.
