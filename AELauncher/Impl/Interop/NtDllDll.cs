using System.Runtime.InteropServices;

namespace ww898.AELauncher.Impl.Interop
{
  internal static unsafe class NtDllDll
  {
    private const string LibraryName = "ntdll.dll";

    [DllImport(LibraryName, ExactSpelling = true, SetLastError = false)]
    public static extern int RtlQueryElevationFlags(uint* pFlags);
  }
}