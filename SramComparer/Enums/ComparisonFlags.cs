using System;

namespace SramComparer.Enums
{
    [Flags]
    public enum ComparisonFlags: uint
    {
        NonGameBuffer = 1 << 0,
        WholeGameBuffer = 1 << 1
    }
}
