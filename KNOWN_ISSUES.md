# Known issues and troubleshooting

1. **Built-in vs custom keyboard:** `Start Keyboard.bat` opens the official Windows On-Screen Keyboard, not the custom ScreenKey C# interface. Run `Start Custom ScreenKey.bat` for the custom interface.
2. **Must extract the archive:** Do not run BAT files directly from ZIP preview. Extract the folder first.
3. **Custom C# compile may fail:** If the .NET Framework compiler is unavailable or reports an error, check `ScreenKey-build.log`. The launcher falls back to the Windows keyboard.
4. **Protected or elevated apps:** Windows may prevent simulated input into administrator or protected apps.
5. **Custom app testing:** We haven't verified the custom C# keyboard on every Windows configuration. User feedback confirms the revised package worked on their PC, but does not identify which launcher they used.
6. **Windows-only:** Not designed for macOS, Linux or mobile.

## Reporting a bug

Share the ScreenKey version, Windows version, launch method, what you expected, what happened, and a non-sensitive part of the error log. Never include your password, key, or private personal data.