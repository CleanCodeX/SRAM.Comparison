using System;
using System.Linq;

namespace SramCommons.Extensions
{
    public static class NumberExtensions
    {
        public static ushort ReverseBytes(this ushort source) => BitConverter.ToUInt16(BitConverter.GetBytes(source).Reverse().ToArray());
        public static short ReverseBytes(this short source) => BitConverter.ToInt16(BitConverter.GetBytes(source).Reverse().ToArray());

        public static uint ReverseBytes(this uint source) => BitConverter.ToUInt32(BitConverter.GetBytes(source).Reverse().ToArray());
        public static int ReverseBytes(this int source) => BitConverter.ToInt32(BitConverter.GetBytes(source).Reverse().ToArray());

        public static string PadLeft(this ushort source) => source.ToString().PadLeft(5);
        public static string PadLeft(this short source) => source.ToString().PadLeft(6);
    }
}
