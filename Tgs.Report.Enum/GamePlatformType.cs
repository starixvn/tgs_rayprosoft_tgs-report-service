using System;

namespace Tgs.Report.Enum;

[Flags]
public enum GamePlatformType : byte
{
    Desktop = 1,
    Mobile = 2,
    DesktopAndMobile = Desktop | Mobile
}