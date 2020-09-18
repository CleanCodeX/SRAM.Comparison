using System;
using System.Diagnostics;
using App.Commons.Extensions;
using SramCommons.Models;
using SramComparer.Helpers;
using SramComparer.Properties;

namespace SramComparer.Services
{
    public abstract class SramComparerBase<TSramFile, TSramGame> : ISramComparer<TSramFile, TSramGame>
        where TSramFile : SramFileBase, ISramFile<TSramGame>
        where TSramGame : struct
    {
        protected IConsolePrinter ConsolePrinter { get; }

        protected SramComparerBase() : this(ServiceCollection.ConsolePrinter) { }
        protected SramComparerBase(IConsolePrinter consolePrinter) => ConsolePrinter = consolePrinter;

        public abstract int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);
        public abstract int CompareGame(TSramGame currGame, TSramGame compGame, IOptions options);

        protected virtual int CompareByte(string bufferName, int bufferOffset, byte currValue, byte compValue, bool writeToConsole = true)
        {
            if (Equals(compValue, currValue)) return 0;

            var byteCount = BitConverter.GetBytes(currValue).Length;

            if (!writeToConsole) return byteCount;

            OnPrintBufferInfo(bufferName, bufferOffset, 2);
            OnPrintComparison(0, null, currValue, compValue);
            OnStatusBytesChanged(byteCount);

            return byteCount;
        }

        protected virtual int CompareUShort(string bufferName, int bufferOffset, ushort currValue, ushort compValue, bool writeToConsole = true)
        {
            if (Equals(compValue, currValue)) return 0;

            var byteCount = BitConverter.GetBytes(currValue).Length;

            if (!writeToConsole) return byteCount;

            ConsoleHelper.EnsureMinConsoleWidth(175);
            OnPrintBufferInfo(bufferName, bufferOffset, 2);
            OnPrintComparison(0, null, currValue, compValue);
            OnStatusBytesChanged(byteCount);

            return byteCount;
        }

        protected virtual int CompareByteArray(string bufferName, int bufferOffset, Span<byte> currValues, Span<byte> compValues, bool writeToConsole = true, Func<int, string?>? offsetNameCallback = null)
        {
            var byteCount = 0;

            Debug.Assert(currValues.Length == compValues.Length);

            for (var offset = 0; offset < currValues.Length; offset++)
            {
                var currValue = currValues[offset];
                var compValue = compValues[offset];

                if (currValue == compValue) continue;

                if (byteCount == 0 && writeToConsole)
                    OnPrintBufferInfo(bufferName, bufferOffset, compValues.Length);

                ++byteCount;

                if (!writeToConsole) continue;

                string? offsetName = null;
                var tempOffsetName = offsetNameCallback?.Invoke(offset);
                if (tempOffsetName is not null)
                    offsetName += $"{tempOffsetName}".PadRight(28);

                OnPrintComparison(offset, offsetName, currValue, compValue);
            }

            if (byteCount == 0 || !writeToConsole) return byteCount;

            OnStatusBytesChanged(byteCount);

            return byteCount;
        }

        protected virtual void OnPrintComparison(int offset, string? offsetName, ushort currValue, ushort compValue) => ConsolePrinter.PrintComparison(" ".Repeat(6), offset, offsetName, currValue, compValue);

        protected virtual void OnPrintComparison(int offset, string? offsetName, byte currValue, byte compValue) => ConsolePrinter.PrintComparison(" ".Repeat(6), offset, offsetName, currValue, compValue);

        protected virtual void OnPrintBufferInfo(string bufferName, int bufferOffset, int byteCount) => ConsolePrinter.PrintBufferInfo(bufferName, bufferOffset, byteCount);

        protected virtual void OnStatusBytesChanged(int byteCount)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ".Repeat(6) + Resources.StatusBytesChangedTemplate, byteCount);
            Console.ResetColor();
        }
    }
}