using System;

namespace SramComparer.Extensions
{
	public static class StringExtensions
	{
		public static int ParseSaveSlotId(this string source)
		{
			int.TryParse(source, out var result);

			return result;
		}
		
		public static bool ParseToBool(this string source) =>
			int.TryParse(source, out var intValue) 
				? Convert.ToBoolean(intValue) 
				: Convert.ToBoolean(source);
	}
}