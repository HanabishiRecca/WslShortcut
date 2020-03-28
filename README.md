# WSL Shortcut

The program allows you to run **WSL** commands directly in Windows.

This is useful when you want to use **WSL** commands from **cmd** or use **WSL** `git`/`node` in **Visual Studio Code**.

### [Releases](../../releases)

### Usage

- Rename `WslShortcut.exe` to desired command name, e.g. `git.exe`, `node.exe`, `ls.exe` etc. You can make a renamed copy for every command you want.
- Place this executable(s) to some **PATH** directory (`Windows`, `System32` or make your own).
- Now you can use it directly in **cmd** (e.g. `git status`, `node -v`, `ls -la`), **VS Code** should find `git`/`node` automatically.
