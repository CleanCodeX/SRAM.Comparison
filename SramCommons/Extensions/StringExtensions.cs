using System;

namespace SramCommons.Extensions
{
    public static class StringExtensions
    {
        public static string FormatStruct(this string source) => source
            .Replace(Environment.NewLine + Environment.NewLine, " | ")
            .Replace(Environment.NewLine, ", ");
    }
}