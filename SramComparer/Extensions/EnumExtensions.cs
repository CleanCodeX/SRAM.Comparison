using System;
using SramComparer.Properties;

namespace SramComparer.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFlagsString<T>(this T source) where T: Enum => 
            Equals(source, default(T)) ? Resources.None : source.ToString();
    }
}