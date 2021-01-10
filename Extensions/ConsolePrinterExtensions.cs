using System;
using SramComparer.Services;

namespace SramComparer.Extensions
{
	public static class ConsolePrinterExtensions
	{
		public static void PrintSectionHeader(this IConsolePrinter source, string headerText, ConsoleColor color = ConsoleColor.Yellow)
		{
			source.PrintSectionHeader();
			source.PrintColoredLine(color, headerText);
			source.ResetColor();
		}
	}
}
