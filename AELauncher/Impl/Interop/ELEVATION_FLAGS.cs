using System;

namespace ww898.AELauncher.Impl.Interop
{
  [Flags]
  internal enum ELEVATION_FLAGS : uint
  {
    ELEVATION_UAC_ENABLED = 0x1,
    ELEVATION_VIRTUALIZATION_ENABLED = 0x2,
    ELEVATION_INSTALLER_DETECTION_ENABLED = 0x4
  }
}