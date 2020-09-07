using System;

namespace SramCommons.Extensions
{
    public static class EnumExtensions
    {
        public static int ToIndex<TEnum>(this TEnum source)
            where TEnum: struct, Enum => (int)(object)source - 1;
    }
}