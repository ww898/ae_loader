# Installation

1. Copy `<architecture>\AELoader.dll` to `%WinDir%\System32`
2. Run `AELoader.Enable.reg` to activate the injection. To deactivate run  `AELoader.Disable.reg`.
3. Create `%ProgramData%\AELoader.<pid>.ini`. Where pid is unsigned decimal value. The pid should be the parent of `%WinDir%\System32\recdisc.exe`.
4. Run `%WinDir%\System32\recdisc.exe` to execute the command line from `%ProgramData%\AELoader.<pid>.ini`.

`AELoader.ini` format:
```
[AELoader]
EnableEventLogs=<The value 1 activates event logs>
CommandLine=<command line to execute under elevated account>
```

