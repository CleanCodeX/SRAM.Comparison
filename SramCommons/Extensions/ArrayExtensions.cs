using System;
using System.Text;

namespace SramCommons.Extensions
{
    public static class ArrayExtensions
    {
        public static string FormatAsString(this byte[] source)
        {
            var sb = new StringBuilder(source.Length);

            for (var i = 0; i < source.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                sb = sb.Append(source[i]);
            }

            return sb.ToString();
        }

        public static string GetString(this byte[] buffer)
        {
            var chars = new char[buffer.Length / sizeof(char)];
            Buffer.BlockCopy(buffer, 0, chars, 0, buffer.Length);
            return new string(chars);
        }

        public static string GetString(this char[] buffer)
        {
            var chars = new char[buffer.Length];
            Buffer.BlockCopy(buffer, 0, chars, 0, buffer.Length);
            return new string(chars);
        }

        public static string GetStringAscii(this byte[] buffer)
        {
            var count = Array.IndexOf<byte>(buffer, 0, 0);
            if (count < 0) count = buffer.Length;
            return Encoding.ASCII.GetString(buffer, 0, count);
        }
    }
}
