using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SramCommons.Models.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
    [DebuggerDisplay("{ToString(),nq}")]
    public struct ThreeByteUint
    {
        public uint Value
        {
            get { return BitConverter.ToUInt32(new[] {ValueByte1, ValueByte2, ValueByte3, byte.MinValue}); }
            set
            {
                var bytes = BitConverter.GetBytes(value);
                ValueByte1 = bytes[0];
                ValueByte2 = bytes[1];
                ValueByte3 = bytes[2];
            }
        }

        public byte ValueByte1;
        public byte ValueByte2;
        public byte ValueByte3;

        public override string ToString() => Value.ToString();
    }
}