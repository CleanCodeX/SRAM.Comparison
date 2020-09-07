using System;
using System.Text;

namespace SramCommons.Extensions
{
    public static class IntExtensions
    {
        public static string FormatBinary(this byte theNumber, int minimumDigits) => InternalFormatBinary(theNumber, minimumDigits);
        public static string FormatBinary(this int theNumber, int minimumDigits) => InternalFormatBinary(theNumber, minimumDigits);
        public static string FormatBinary(this short theNumber, int minimumDigits) => InternalFormatBinary(theNumber, minimumDigits);
        public static string FormatBinary(this ushort theNumber, int minimumDigits) => InternalFormatBinary(theNumber, minimumDigits);
        public static string FormatBinary(this uint theNumber, int minimumDigits) => InternalFormatBinary(theNumber, minimumDigits);

        private static string InternalFormatBinary(long theNumber, int minimumDigits)
        {
            var result = Convert.ToString(theNumber, 2).PadLeft(minimumDigits, '0');

            if (minimumDigits <= 8 || minimumDigits % 8 > 0)
                return SplitResult(0);

            var sb = new StringBuilder();
            
            for (var i = 0; i < result.Length; i += 8)
            {
                var byteBitString = SplitResult(i);

                if (i > 0)
                    sb.Append(", ");

                sb.Append($"{byteBitString}");
            }

            return sb.ToString().TrimEnd();

            string SplitResult(int i) => result.Substring(i, 4) + "-" + result.Substring(i + 4, 4);
        }
    }
}
