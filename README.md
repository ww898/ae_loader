# Description

This library automatically elevate administrator account. No more "Yes/No" modal dialog will be shown.
Tested on Windows 10 Pro 1909 build 18363.535.

# Restrictions

This library doesn't work:
- on non-administrator account
- on computers with secure boot
- in DRM-processes
- in special list of system processes, such as:
  * Windows Defender
  * Windows Software Licensing service
  * Microsoft Hyper-V critical processes (vmms.exe and vmwp.exe)

# Installation

1. Copy `<architecture>\AELoader.dll` to `%WinDir%\System32`and `%WinDir%\SysWOW64`.
2. Run `AELoader.Enable.reg` to activate the injection. To deactivate run  `AELoader.Disable.reg`.

# Usage

1. Use `ww898.AELauncher.AEProcess.Start()` from `ww898.AELauncher.<version>.nupkg`.
2. Create `%ProgramData%\AELoader.<pid>.ini`, where `pid` is unsigned decimal value of parent process of `%WinDir%\System32\recdisc.exe`. Run `%WinDir%\System32\recdisc.exe`.

`AELoader.<pid>.ini` format:
```
[AELoader]
EnableEventLogs=<The value 1 activates event logs for success operations>
CommandLine=<command line to execute under elevated account>
```
