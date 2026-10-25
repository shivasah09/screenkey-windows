SCREENKEY — ON-SCREEN KEYBOARD FOR WINDOWS
=========================================

HOW TO RUN
1. Extract the whole ZIP to a folder.
2. Double-click "Start Keyboard.bat" or "Launch ScreenKey.vbs".
3. Click in a text field in Chrome, Notepad, Word, or another application.
4. Click the keyboard's buttons to type into that application.

COMPATIBILITY
- Designed for Windows 10 and Windows 11.
- No Python, Visual Studio, installation, or internet connection required.
- Uses Windows PowerShell 5.1, Windows Forms, and the Windows SendInput API.
- Works with regular desktop apps and most browser text fields.
- Some games, apps running as administrator, and protected Windows login/UAC
  screens do not accept synthetic keystrokes. This is a Windows security limit.

FEATURES
- Dark theme; draggable and always on top by default.
- QWERTY letters, numbers, punctuation, F1-F12.
- Shift, Caps, Ctrl, Alt, Win, Tab, Enter, Backspace, Delete, arrow keys.
- Click Ctrl then A/C/V, or Shift then an arrow, for keyboard shortcuts.
- Local-only: no logging, key history, networking, or telemetry.
- SIZE: toggle normal/compact; PIN: toggle always-on-top.
- 100%: cycle window transparency. —: minimize. ×: close.

TIPS
- Shift/Ctrl/Alt/Win are one-shot: they switch off after the next key.
- Caps Lock remains enabled until you click it again.
- If nothing types, click the target text field once, then use ScreenKey.
- You can edit ScreenKey.cs yourself. The launcher recompiles it at start.
- Do not rename or separate the four launcher/source files.

ALTERNATIVE
Windows already provides a built-in keyboard: press Win + Ctrl + O.

SECURITY / TROUBLESHOOTING
- ScreenKey uses Windows keyboard injection but does not capture or save keys.
- If it fails to open, read the ScreenKey-error.log created in this folder.
- Running downloaded scripts may prompt Windows security warnings.
  Inspect the source before allowing it if you have concerns.