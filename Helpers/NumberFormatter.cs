using System;
using IO.Extensions;

namespace SRAM.Comparison.Helpers
{
	public class NumberFormatter
	{
		public static string GetByteValueRepresentations(byte value) => $"{value,3:D3} [x{value,2:X2}] [{value.FormatBinary(8)}]";
		public static string GetByteValueChangeRepresentations(byte currValue, byte compValue)
		{
			var change = currValue - compValue;
			var absChange = (uint)Math.Abs(change);
			var changeString = $"{absChange,3:###}";
			return $"{changeString,3} [x{absChange,2:X2}] [{absChange.FormatBinary(8)}]";
		}
	}
}
