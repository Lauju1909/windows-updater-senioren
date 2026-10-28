@echo off
echo Kompiliere Computer-Aktualisierer.exe...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:"%~dp0Computer-Aktualisierer.exe" /win32manifest:"%~dp0app.manifest" /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\System.Speech.dll" "%~dp0Program.cs"
if %errorlevel% equ 0 (
    echo.
    echo Erfolgreich kompiliert: Computer-Aktualisierer.exe
) else (
    echo.
    echo Fehler beim Kompilieren!
)
pause
