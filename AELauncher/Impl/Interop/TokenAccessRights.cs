using System;

namespace ww898.AELauncher.Impl.Interop
{
  [Flags]
  internal enum TokenAccessRights : uint
  {
    TOKEN_QUERY = 0x0008
  }
}