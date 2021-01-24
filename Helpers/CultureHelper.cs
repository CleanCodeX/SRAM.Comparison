using System;
using System.Globalization;
using SRAM.Comparison.Services;

namespace SRAM.Comparison.Helpers
{
	public static class CultureHelper
	{
		public static void TrySetCulture(string culture) => TrySetCulture(culture, ServiceCollection.ConsolePrinter);
		public static void TrySetCulture(string culture, IConsolePrinter consolePrinter)
		{
			try
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
			}
			catch (Exception ex)
			{
				consolePrinter.PrintError(ex.Message);
				consolePrinter.PrintSectionHeader();
			}
		}
	}
}
