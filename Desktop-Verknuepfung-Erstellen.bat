@echo off
chcp 65001 >nul
echo Erstelle Verknuepfung auf dem Desktop...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Desktop-Verknuepfung-Erstellen.ps1"
echo.
echo Fertig! Sie finden jetzt das Symbol "Computer Aktualisieren" auf Ihrem Schreibtisch (Desktop).
pause
