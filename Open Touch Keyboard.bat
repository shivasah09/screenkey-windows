@echo off
setlocal
if exist "%CommonProgramFiles%\microsoft shared\ink\TabTip.exe" (
    start "" "%CommonProgramFiles%\microsoft shared\ink\TabTip.exe"
) else (
    start "" "%SystemRoot%\System32\osk.exe"
)