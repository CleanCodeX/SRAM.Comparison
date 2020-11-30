using SramCommons.Extensions;

namespace SramComparer.Helpers
{
	public class NumberFormatter
	{
		public static string GetByteValueRepresentations(byte value) => $"{value,3:D3} [x{value,2:X2}] [{value.FormatBinary(8)}]";
	}
}
