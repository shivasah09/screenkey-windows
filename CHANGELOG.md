# Changelog — ScreenKey

Versions follow a simple `major.minor.patch` style from now on. Every update should describe **new features**, **fixes**, **known issues**, and **test status**.

## v1.1.0 — 2026-10-25 — Fixed launcher (recommended)

### New
- Added reliable `Start Keyboard.bat` to open Windows' built-in `osk.exe`.
- Added `Start Custom ScreenKey.bat` as a separate option to compile and run the original C# keyboard.
- Added visible compilation errors and `ScreenKey-build.log` for troubleshooting.
- Added an automatic built-in-keyboard fallback if the C# build fails.
- Added `Open Touch Keyboard.bat` as another Windows keyboard option.

### Fixed / changed
- Avoided reliance on the original PowerShell/VBS path for the default entry point.
- Made keyboard startup troubleshooting easier instead of silently failing.

### Known issues
- The optional custom keyboard has **not** been independently tested on a physical Windows 10/11 PC here.
- Custom simulated keystrokes may not work in elevated or protected apps.
- The C# source is the **same source** as v1.0; this update focuses on reliable launching and diagnostics.

### Test notes
- A user reported that the revised keyboard package was working on their PC.
- Package contents were inspected; Windows compilation/runtime testing was not performed in this environment.

## v1.0.0 — 2026-10-25 — Initial prototype

### New
- First dark custom C# WinForms keyboard with keys, modifier support and floating UI.
- Original launchers used PowerShell and Visual Basic Script.

### Known issues / user feedback
- User reported: **“not working.”** The failure was not narrowed down to startup versus typing, so no unsupported root cause is claimed.
- Superseded by v1.1.0; this version is kept for development history, not recommended for end users.