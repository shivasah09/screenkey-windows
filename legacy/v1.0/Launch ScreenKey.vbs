' Double-click this file to launch ScreenKey without a console window.
Set fso = CreateObject("Scripting.FileSystemObject")
folder = fso.GetParentFolderName(WScript.ScriptFullName)
cmd = "powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -STA -File """ & folder & "\ScreenKey.ps1"""
Set shell = CreateObject("WScript.Shell")
shell.Run cmd, 0, False