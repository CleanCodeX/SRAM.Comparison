using System;
using System.Globalization;
using SramComparer.Services;

namespace SramComparer.Helpers
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
