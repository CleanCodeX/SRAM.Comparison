using System;

namespace SramComparer.Enums
{
    [Flags]
    public enum ComparisonFlags
    {
        NonGameBuffer = 1 << 0,
        WholeGameBuffer = 1 << 1
    }
}
