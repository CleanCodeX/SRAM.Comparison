using System;
using SramComparer.Services;

namespace SramComparer.Extensions
{
    public static class ConsolePrinterExtensions
    {
        public static void PrintSectionHeader(this IConsolePrinter source, string headerText, ConsoleColor color = ConsoleColor.Yellow)
        {
            source.PrintSectionHeader();
            Console.ForegroundColor = color;
            Console.WriteLine(headerText);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
