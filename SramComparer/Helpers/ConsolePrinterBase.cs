using System;
using App.Commons.Extensions;
using SramCommons.Extensions;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Helpers
{
    public abstract class ConsolePrinterBase
    {
        public static void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength)
        {
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ".Repeat(4) + @$"[ {Res.Section} ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(bufferName);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" | ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($@"{Res.Offset} ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(bufferOffset + $@" [x{bufferOffset:X}]");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" | ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($@"{Res.Size} ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(bufferLength);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" ]");

            Console.ResetColor();
        }

        public static void PrintComparison(string ident, ushort offset, string? offsetName, ushort currValue, ushort compValue, int bitLength)
        {
            var sign = GetSign((short)(currValue - compValue));
            var change = currValue - compValue;
            var absChange = Math.Abs(change);
            var changeString = $"{absChange,5:###}";
            var isNegativechange = change < 0;

            var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
            var compText = $"{compValue,3:D5} [x{compValue,2:X4}] [{compValue.FormatBinary(bitLength)}]";
            var currText = $"{currValue,3:D5} [x{currValue,2:X4}] [{currValue.FormatBinary(bitLength)}]";
            var changeText = $"{changeString,5} [x{absChange,2:X4}] [{absChange.FormatBinary(bitLength)}]";

            PrintComparisonIdentification(ident);
            PrintOffsetValues(offsetText, offsetName);
            PrintCompValues(isNegativechange, compText);
            PrintCurrValues(isNegativechange, currText);
            PrintChangeValues(isNegativechange, sign, changeText);
        }

        public static void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue)
        {
            var sign = GetSign((short)(currValue - compValue));
            var change = currValue - compValue;
            var absChange = Math.Abs(change);
            var changeString = $"{absChange,3:###}";
            var isNegativechange = change < 0;

            var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
            var compText = $"{compValue,3:D3} [x{compValue,2:X2}] [{compValue.FormatBinary(8)}]";
            var currText = $"{currValue,3:D3} [x{currValue,2:X2}] [{currValue.FormatBinary(8)}]";
            var changeText = $"{changeString,3} [x{absChange,2:X2}] [{absChange.FormatBinary(8)}]";

            PrintComparisonIdentification(ident);
            PrintOffsetValues(offsetText, offsetName);
            PrintCompValues(isNegativechange, compText);
            PrintCurrValues(isNegativechange, currText);
            PrintChangeValues(isNegativechange, sign, changeText);
        }

        public static void WriteNewSectionHeader()
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($@"===[ {DateTime.Now.ToLongTimeString()} ]====================================================");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void EnsureMinConsoleWidth(int minWidth)
        {
            if (Console.WindowWidth < minWidth)
                Console.WindowWidth = minWidth;
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(ex);
            Console.ResetColor();
        }

        private static void PrintOffsetValues(string offsetText, string? offsetName)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($@"{Res.Offset} ");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(offsetText);

            if (offsetName is null) return;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(@" => ");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(offsetName);
        }

        private static void PrintCompValues(bool isNegativeChange, string compText)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" | ");
            Console.ForegroundColor = isNegativeChange ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($@"{Res.CompShort} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(compText);
        }

        private static void PrintCurrValues(bool isNegativeChange, string currText)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(@" => ");
            Console.ForegroundColor = isNegativeChange ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write($@"{Res.CurrShort} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(currText);
        }

        private static void PrintChangeValues(bool isNegativeChange, string sign, string changeText)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(@" = ");

            Console.ForegroundColor = isNegativeChange ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write($@"{Res.ChangeShort} ");
            Console.Write(sign);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(changeText);
        }

        private static string GetSign(short value) => Math.Sign(value) < 0 ? "(-)" : "(+)";

        private static void PrintComparisonIdentification(string ident) => Console.Write($@"{ident}=> ");

        protected static void WriteSettingName(string settingName, string? cmdArg = null)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(settingName.PadRight(28) + @":");

            if (cmdArg is null) return;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(cmdArg);
        }

        protected static void WriteValue(object value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" " + value);
        }
    }
}