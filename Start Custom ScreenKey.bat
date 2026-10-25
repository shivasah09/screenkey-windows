@echo off
setlocal
cd /d "%~dp0"
title ScreenKey Custom Keyboard - Launcher
REM Compile the included source into a Windows application using built-in .NET Framework tools.
set "CSC=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" goto :nocsc
if exist "%~dp0ScreenKey.exe" goto :run

echo Compiling ScreenKey on this PC...
"%CSC%" /nologo /target:winexe /optimize+ /codepage:65001 /out:"%~dp0ScreenKey.exe" /reference:System.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "%~dp0ScreenKey.cs" > "%~dp0ScreenKey-build.log" 2>&1
if errorlevel 1 goto :buildfail

:run
echo Starting custom keyboard...
start "" "%~dp0ScreenKey.exe"
if errorlevel 1 goto :runfail
exit /b 0

:nocsc
echo This PC does not have the built-in .NET Framework compiler needed for custom ScreenKey.
echo Opening the standard Windows On-Screen Keyboard instead.
start "" "%SystemRoot%\System32\osk.exe"
pause
exit /b 1

:buildfail
echo.
echo The custom keyboard could not be compiled. Here is the error:
type "%~dp0ScreenKey-build.log"
echo.
echo A log has been saved to ScreenKey-build.log
start "" "%SystemRoot%\System32\osk.exe"
pause
exit /b 1

:runfail
echo Could not launch the custom keyboard. Opening Windows keyboard instead.
start "" "%SystemRoot%\System32\osk.exe"
pause
exit /b 1