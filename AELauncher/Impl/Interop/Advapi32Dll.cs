using System.Runtime.InteropServices;

namespace ww898.AELauncher.Impl.Interop
{
  internal static unsafe class Advapi32Dll
  {
    private const string LibraryName = "advapi32.dll";

    [DllImport(LibraryName, ExactSpelling = true, SetLastError = true)]
    public static extern int OpenProcessToken(void* ProcessHandle, uint DesiredAccess, void** TokenHandle);

    [DllImport(LibraryName, ExactSpelling = true, SetLastError = true)]
    public static extern int GetTokenInformation(void* TokenHandle, int TokenInformationClass, void* TokenInformation, uint TokenInformationLength, uint* ReturnLength);
  }
}