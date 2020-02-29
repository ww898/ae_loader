using System.Runtime.InteropServices;

namespace ww898.AELauncher.Impl.Interop
{
  internal static unsafe class Kernel32Dll
  {
    private const string LibraryName = "kernel32.dll";

    [DllImport(LibraryName, ExactSpelling = true, SetLastError = true)]
    public static extern int CloseHandle(void* hObject);
  }
}