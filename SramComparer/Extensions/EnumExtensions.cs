using System;
using SramComparer.Properties;

namespace SramComparer.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFlagsString(this Enum source) => source.ToString() == "0" ? Resources.None : source.ToString();
    }
}