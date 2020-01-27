if ($PSVersionTable.PSVersion.Major -lt 3) {
  throw "PS Version $($PSVersionTable.PSVersion) is below 3.0."
}

Set-StrictMode -Version Latest
$ErrorActionPreference = [System.Management.Automation.ActionPreference]::Stop
$script:VerbosePreference = "Continue"

$_IniFile = "$env:ProgramData\AELoader.$([uint32]$pid).ini"
Write-Host "Configuration file: $_IniFile"
Copy-Item AELoader.ini -Destination $_IniFile

& recdisc.exe

Start-Sleep 1
if (Test-Path $_IniFile -PathType Leaf) {
  Remove-Item $_IniFile
  Write-Host "Injection was failed" -ForegroundColor Red
}
else {
  Write-Host "Success"  -ForegroundColor Green
}

