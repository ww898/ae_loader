using System;
using System.ComponentModel;
using System.Diagnostics;
using ww898.AELauncher.Impl.Interop;

namespace ww898.AELauncher.Impl
{
  internal static class ElevationUtil
  {
    public static unsafe bool IsUacEnabled()
    {
      if (Environment.OSVersion.Version < new Version(6, 0))
        return false;
      uint elevationFlags;
      var error = NtDllDll.RtlQueryElevationFlags(&elevationFlags);
      if (error != (int) WinError.ERROR_SUCCESS)
        throw new Win32Exception(error, "Can't get elevation flags");
      return ((ELEVATION_FLAGS) elevationFlags & ELEVATION_FLAGS.ELEVATION_UAC_ENABLED) != 0;
    }

    public static unsafe TOKEN_ELEVATION_TYPE GetElevationType(Process process)
    {
      if (process == null)
        throw new ArgumentNullException(nameof(process));
      if (Environment.OSVersion.Version < new Version(6, 0))
        return TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;
      void* handle;
      if (Advapi32Dll.OpenProcessToken(process.Handle.ToPointer(), (uint) TokenAccessRights.TOKEN_QUERY, &handle) == 0)
        throw new Win32Exception("Can't open process token");
      try
      {
        TOKEN_ELEVATION_TYPE tet;
        var size = (uint) sizeof(TOKEN_ELEVATION_TYPE);
        if (Advapi32Dll.GetTokenInformation(handle, (int) TOKEN_INFORMATION_CLASS.TokenElevationType, &tet, size, &size) == 0)
          throw new Win32Exception("Can't get elevation type");
        return tet;
      }
      finally
      {
        Kernel32Dll.CloseHandle(handle);
      }
    }
  }
}