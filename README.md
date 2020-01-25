# Installation

1. Copy `<architecture>\AELoader.dll` to `%WinDir%\System32`
2. Update or create `%ProgramData%\AELoader.ini`.
3. Run `AELoader.Enable.reg` to activate the injection. To deactivate run  `AELoader.Disable.reg`.
4. Run `%WinDir%\System32\recdisc.exe` to execute the command line from `%ProgramData%\AELoader.ini`.

`AELoader.ini` format:
```
[AELoader]
EnableEventLogs=<The value 1 activates event logs>
CommandLine=<command line to execute under elevated account>
```

