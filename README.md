# ScreenKey — On-Screen Keyboard for Windows

**Windows 10/11 • Open-source C# / WinForms • Portable launchers • No cloud or Python required**

ScreenKey is a small on-screen keyboard project made for Windows. It includes a dependable launcher for the **Windows built-in On-Screen Keyboard** and an optional **custom dark floating keyboard** written in C#.

> **Latest: v1.1 (Fixed Launcher).** The user reported that the revised keyboard package worked on their PC. This confirms their experience, **not** an independently verified test of every custom-keyboard feature or Windows version.

## Download

- **Recommended v1.1:** [ScreenKey-v1.1-Fixed.zip](downloads/ScreenKey-v1.1-Fixed.zip)
- Historical v1.0 (reported not working): [ScreenKey-v1.0-Original.zip](downloads/ScreenKey-v1.0-Original.zip) — **not recommended**.

If downloading from the GitHub repository, open the `downloads` folder and select the v1.1 ZIP, then click **Download raw file**. Future versioned releases may be published under GitHub **Releases**.

## Quick start (v1.1)

1. Download and **extract** `ScreenKey-v1.1-Fixed.zip` (right-click → **Extract All**).
2. Open the extracted `ScreenKey_Fixed` folder.
3. Run **`Start Keyboard.bat`** for the reliable **built-in Windows On-Screen Keyboard** (`osk.exe`).
4. Or run **`Start Custom ScreenKey.bat`** to try our **custom dark keyboard**.
5. Click the text field you want to type into. For the built-in keyboard you can also press **Win + Ctrl + O**.

**Important:** Running `Start Keyboard.bat` launches the keyboard included with Windows. To test ScreenKey's original C# interface specifically, use **`Start Custom ScreenKey.bat`**. These are different experiences.

## Custom keyboard features (C# source)

- Dark theme with draggable floating window
- Letters, numbers, symbols, function keys and arrows
- Shift, Caps Lock, Ctrl, Alt and Windows key input
- Always-on-top toggle, compact mode and adjustable transparency
- Uses Windows `SendInput` for simulated typing
- Local-only code; no analytics, network API calls or key history in the supplied source

The optional custom launcher compiles `ScreenKey.cs` using the .NET Framework C# compiler when available; it saves errors to `ScreenKey-build.log`, and uses the Windows keyboard as a fallback if compilation fails.

## Versions

| Version | Status | What changed |
|---|---|---|
| **v1.0** | Previous version, user reported a launch/work issue | Original PowerShell + VBS launcher and C# keyboard. The exact failure was not established. |
| **v1.1** | Recommended, user reported working | Reliable Windows `osk.exe` launcher, optional .NET compiler launcher, compile error log and fallback. The underlying `ScreenKey.cs` is unchanged from v1.0. |

See [CHANGELOG.md](CHANGELOG.md) for updates, and [KNOWN_ISSUES.md](KNOWN_ISSUES.md) for limitations.

## Repo layout

- `Start Keyboard.bat` — recommended Windows keyboard launcher
- `Start Custom ScreenKey.bat` — optional C# keyboard builder + launcher
- `ScreenKey.cs` — original custom WinForms source
- `Open Touch Keyboard.bat` — Windows touch keyboard option
- `downloads/` — downloadable version archives
- `legacy/v1.0/` — original first version, preserved for learning and comparison
- `CHANGELOG.md` — per-version changes
- `KNOWN_ISSUES.md` — known limitations / troubleshooting
- `RELEASE_TEMPLATE.md` — reusable format for future versions

## Privacy and safety

ScreenKey does not intentionally collect or transmit typed data. Check source before running any downloaded script. Avoid typing sensitive information in apps you do not trust. Windows security restrictions prevent simulated input into some administrator/protected apps.

## Development

This is a learning and portfolio project. Contributions and clear bug reports are welcome. Please include your Windows version, launcher used and the relevant **non-sensitive** error message; don't share passwords or personal data.

## License

Released under the [MIT License](LICENSE).