@echo off
setlocal
REM RELIABLE WINDOWS ON-SCREEN KEYBOARD: opens the built-in Windows keyboard.
if exist "%SystemRoot%\System32\osk.exe" (
    start "" "%SystemRoot%\System32\osk.exe"
    exit /b 0
)
if exist "%SystemRoot%\Sysnative\osk.exe" (
    start "" "%SystemRoot%\Sysnative\osk.exe"
    exit /b 0
)
start "" osk.exe
if errorlevel 1 (
    echo Could not open Windows On-Screen Keyboard.
    echo Press Windows + Ctrl + O or search for "On-Screen Keyboard" in Start.
    pause
)