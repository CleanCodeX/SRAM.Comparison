using System;
using SramComparer.Properties;

namespace SramComparer.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFlagsString<T>(this T source) where T: Enum => 
            Equals((int)(object)source, 0) ? Resources.None : source.ToString();
    }
}