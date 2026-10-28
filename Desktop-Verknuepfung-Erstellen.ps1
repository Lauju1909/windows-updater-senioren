$WshShell = New-Object -ComObject WScript.Shell
$DesktopPath = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop)
$ShortcutPath = Join-Path -Path $DesktopPath -ChildPath "Computer Aktualisieren.lnk"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TargetExe = Join-Path -Path $ScriptDir -ChildPath "Computer-Aktualisierer.exe"

$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = $TargetExe
$Shortcut.WorkingDirectory = $ScriptDir
$Shortcut.Description = "Mit 1 Klick alle Programme und Windows barrierefrei und sicher aktualisieren"
# Windows Schutzschild Icon
$Shortcut.IconLocation = "$env:SystemRoot\System32\imageres.dll,102"
$Shortcut.Save()

Write-Output "Die Verknuepfung 'Computer Aktualisieren' wurde erfolgreich auf Ihrem Desktop erstellt!"
