using System;
using System.Diagnostics;
using App.Commons.Extensions;
using SramCommons.Extensions;
using SramCommons.Models;
using SramComparer.Properties;

namespace SramComparer.Helpers
{
    public interface ISramComparer<in TSramFile, TSramGame, TGameId>
        where TSramFile : SramFileBase, ISramFile<TSramGame, TGameId>
        where TSramGame : struct
        where TGameId : struct, Enum
    {
        void CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);
    }

    public abstract class SramComparerBase<TSramFile, TSramGame, TGameId> : ISramComparer<TSramFile, TSramGame, TGameId>
        where TSramFile : SramFileBase, ISramFile<TSramGame, TGameId>
        where TSramGame : struct
        where TGameId: struct, Enum
    {
        public abstract void CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);
        protected abstract int CompareGame(TSramGame currGame, TSramGame compGame, IOptions options);

        protected static int CompareByte(string bufferName, int bufferOffset, byte currValue, byte compValue, bool writeToConsole = true)
        {
            if (Equals(compValue, currValue)) return 0;

            var byteCount = BitConverter.GetBytes(currValue).Length;
            var bitLength = byteCount * 8;

            if (writeToConsole)
            {
                ConsolePrinterBase.PrintBufferInfo(bufferName, bufferOffset, 2);

                Console.WriteLine();
                Console.ResetColor();
            }

            if (writeToConsole) ConsolePrinterBase.PrintComparison(" ".Repeat(6), 0, null, currValue, compValue, bitLength);

            if (!writeToConsole) return byteCount;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ".Repeat(6) + Resources.StatusBytesChangedTemplate, byteCount);
            Console.ResetColor();

            return byteCount;
        }

        protected static int CompareUShort(string bufferName, int bufferOffset, ushort currValue, ushort compValue, bool reverseByteOrder = false, bool writeToConsole = true)
        {
            if (Equals(compValue, currValue)) return 0;

            if (reverseByteOrder)
            {
                currValue = currValue.ReverseBytes();
                compValue = compValue.ReverseBytes();
            }

            var byteCount = BitConverter.GetBytes(currValue).Length;
            var bitLength = byteCount * 8;

            if (writeToConsole)
            {
                ConsolePrinterBase.EnsureMinConsoleWidth(175);

                ConsolePrinterBase.PrintBufferInfo(bufferName, bufferOffset, 2);

                if (reverseByteOrder)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(@$" [{Resources.ReversedByteOrder}]");
                }

                Console.WriteLine();
                Console.ResetColor();
            }

            if (writeToConsole) ConsolePrinterBase.PrintComparison(" ".Repeat(6), 0, null, currValue, compValue, bitLength);

            if (!writeToConsole) return byteCount;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ".Repeat(6) + Resources.StatusBytesChangedTemplate, byteCount);
            Console.ResetColor();

            return byteCount;
        }

        protected static int CompareByteArray(string bufferName, int bufferOffset, Span<byte> currValues, Span<byte> compValues, bool writeToConsole = true, Func<int, string?>? offsetNameCallback = null)
        {
            var found = 0;

            Debug.Assert(currValues.Length == compValues.Length);

            for (var offset = 0; offset < currValues.Length; offset++)
            {
                var currValue = currValues[offset];
                var compValue = compValues[offset];

                if (currValue == compValue) continue;

                if (found == 0 && writeToConsole)
                {
                    ConsolePrinterBase.PrintBufferInfo(bufferName, bufferOffset, compValues.Length);
                    Console.WriteLine();
                }

                ++found;

                if (!writeToConsole) continue;

                string? offsetName = null;
                var tempOffsetName = offsetNameCallback?.Invoke(offset);
                if (tempOffsetName is not null)
                    offsetName += $"{tempOffsetName}".PadRight(28);

                ConsolePrinterBase.PrintComparison(" ".Repeat(6), offset, offsetName, currValue, compValue);
            }

            if (found == 0) return 0;
            if (!writeToConsole) return found;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ".Repeat(6) + Resources.StatusBytesChangedTemplate, found);
            Console.ResetColor();

            return found;
        }
    }
}