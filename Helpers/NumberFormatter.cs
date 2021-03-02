using IO.Extensions;

namespace SRAM.Comparison.Helpers
{
	public static class NumberFormatter
	{
		public static string FormatDecHexBin(uint value) => $"{FormatDecHex(value)} [{FormatBin(value)}]";
		public static string FormatDecHex(uint value) => $"{FormatDec(value)} [{FormatHex(value)}]";
		public static string FormatDec(uint value) => $"{value:D9}";
		public static string FormatHex(uint value) => $"x{value:X8}";
		public static string FormatBin(uint value) => value.FormatBinary(32);

		public static string FormatDecHexBin(ushort value) => $"{FormatDecHex(value)} [{FormatBin(value)}]";
		public static string FormatDecHex(ushort value) => $"{FormatDec(value)} [{FormatHex(value)}]";
		public static string FormatDec(ushort value) => $"{value:D5}";
		public static string FormatHex(ushort value) => $"x{value:X4}";
		public static string FormatBin(ushort value) => value.FormatBinary(16);

		public static string FormatDecHexBin(byte value) => $"{FormatDecHex(value)} [{FormatBin(value)}]";
		public static string FormatDecHex(byte value) => $"{FormatDec(value)} [{FormatHex(value)}]";
		public static string FormatDec(byte value) => $"{value:D3}";
		public static string FormatHex(byte value) => $"x{value:X2}";
		public static string FormatBin(byte value) => value.FormatBinary(8);
	}
}
