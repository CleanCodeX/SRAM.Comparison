using System;
using System.Diagnostics;
using System.IO;
using Common.Shared.Min.Extensions;
using IO.Models;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison.Services
{
	/// <summary>
	/// Base class for S-RAM comparison. Needs an actual implementation for a specific game
	/// </summary>
	/// <typeparam name="TSramFile">The S-RAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The S-RAM save slot structure</typeparam>
	public abstract class SramComparerBase<TSramFile, TSaveSlot> : ISramComparer<TSramFile, TSaveSlot>
		where TSramFile : class, IMultiSegmentFile<TSaveSlot>, IRawSave
		where TSaveSlot : struct
	{
		protected int ComparisonConsoleWidth = 157;
		protected string UnknownIdentifier = "unknown";
		protected IConsolePrinter ConsolePrinter { get; }

		protected SramComparerBase() : this(ComparisonServices.ConsolePrinter) { }
		protected SramComparerBase(IConsolePrinter consolePrinter) => ConsolePrinter = consolePrinter;

		/// <inheritdoc cref="ISramComparer{TSramFile,TSaveSlot}.CompareSram(TSramFile, TSramFile, IOptions, TextWriter?)"/>
		public virtual int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options, TextWriter? output = null)
		{
			using (new TemporaryConsoleOutputSetter(output))
				return OnCompareSram(currFile, compFile, options);
		}

		protected abstract int OnCompareSram(TSramFile currFile, TSramFile compFile, IOptions options);

		/// <summary>Compares a single save slot</summary>
		/// <param name="currSlot">The current save slot</param>
		/// <param name="compSlot">The comparison save slot</param>
		/// <param name="options">The options to be used for comparison</param>
		/// <returns>Number of compared bytes changed</returns>
		protected abstract int OnCompareSaveSlot(TSaveSlot currSlot, TSaveSlot compSlot, IOptions options);

		/// <summary>
		/// Compares a single byte
		/// </summary>
		/// <param name="name">The name of the compared buffer</param>
		/// <param name="offset">The buffer's offset at this byte is located</param>
		/// <param name="currValue">The current byte to be compared</param>
		/// <param name="compValue">The comparison byte to be compared</param>
		/// <param name="writeToConsole">Sets if any output should be written to console. Default is true</param>
		/// <param name="isUnknown">Indicates that this offset is considered to 'Unknown'</param>
		/// <returns>1 if the byte has changed, otherwise 0</returns>
		protected virtual int CompareValue(string name, int offset, byte currValue, byte compValue, bool writeToConsole = true, bool isUnknown = false)
		{
			if (Equals(compValue, currValue)) return 0;

			var byteCount = BitConverter.GetBytes(currValue).Length;

			if (!writeToConsole) return byteCount;

			ConsoleHelper.EnsureMinConsoleWidth(ComparisonConsoleWidth);

			OnPrintBufferInfo(name, offset, 2, GetWramOffset(offset));
			OnPrintComparison(0, null, currValue, compValue, isUnknown);
			OnStatusBytesChanged(byteCount);

			return byteCount;
		}

		/// <summary>
		/// Compares a single 2-byte value (UShort)
		/// </summary>
		/// <param name="name">The name of the compared buffer</param>
		/// <param name="offset">The buffer's offset at this ushort is located</param>
		/// <param name="currValue">The current ushort to be compared</param>
		/// <param name="compValue">The comparison ushort to be compared</param>
		/// <param name="writeToConsole">Sets if any output should be written to console. Default is true</param>
		/// <param name="isUnknown">Indicates that this offset is considered to 'Unknown'</param>
		/// <returns>2 if the ushort changed, otherwise 0</returns>
		protected virtual int CompareValue(string name, int offset, ushort currValue, ushort compValue, bool writeToConsole = true, bool isUnknown = false)
		{
			if (Equals(compValue, currValue)) return 0;

			var byteCount = BitConverter.GetBytes(currValue).Length;

			if (!writeToConsole) return byteCount;

			ConsoleHelper.EnsureMinConsoleWidth(ComparisonConsoleWidth);

			OnPrintBufferInfo(name, offset, 2, GetWramOffset(offset));
			OnPrintComparison(0, null, currValue, compValue, isUnknown);
			OnStatusBytesChanged(byteCount);

			return byteCount;
		}
		
		/// <summary>
		/// Compares a single 2-byte value (UShort)
		/// </summary>
		/// <param name="name">The name of the compared buffer</param>
		/// <param name="offset">The buffer's offset at this ushort is located</param>
		/// <param name="currValue">The current ushort to be compared</param>
		/// <param name="compValue">The comparison ushort to be compared</param>
		/// <param name="writeToConsole">Sets if any output should be written to console. Default is true</param>
		/// <param name="isUnknown">Indicates that this offset is considered to 'Unknown'</param>
		/// <returns>2 if the ushort changed, otherwise 0</returns>
		protected virtual int CompareValue(string name, int offset, uint currValue, uint compValue, bool writeToConsole = true, bool isUnknown = false)
		{
			if (Equals(compValue, currValue)) return 0;

			var byteCount = BitConverter.GetBytes(currValue).Length;

			if (!writeToConsole) return byteCount;

			ConsoleHelper.EnsureMinConsoleWidth(ComparisonConsoleWidth);

			OnPrintBufferInfo(name, offset, 2, GetWramOffset(offset));
			OnPrintComparison(0, null, currValue, compValue, isUnknown);
			OnStatusBytesChanged(byteCount);

			return byteCount;
		}

		/// <summary>
		/// Compares a single 2-byte value (UShort)
		/// </summary>
		/// <param name="name">The name of the compared buffer</param>
		/// <param name="offset">The buffer's offset at where this ushort is located</param>
		/// <param name="currValues">The current buffer's bytes to be compared</param>
		/// <param name="compValues">The comparison buffer's bytes to be compared</param>
		/// <param name="writeToConsole">Sets if any output should be written to console. Default is true</param>
		/// <param name="offsetNameCallback">An optional callback function from which the name of a specific offset can be returned</param>
		/// <param name="isUnknown">Indicates that this offset is considered to 'Unknown'</param>
		/// <returns>The amound of bytes changed</returns>
		protected virtual int CompareValue(string name, int offset, ReadOnlySpan<byte> currValues, ReadOnlySpan<byte> compValues, bool writeToConsole = true, Func<int, string?>? offsetNameCallback = null, bool isUnknown = false)
		{
			var byteCount = 0;

			Debug.Assert(currValues.Length == compValues.Length);

			for (var byteOffset = 0; byteOffset < currValues.Length; byteOffset++)
			{
				var currValue = currValues[byteOffset];
				var compValue = compValues[byteOffset];

				if (currValue == compValue) continue;

				if (byteCount == 0 && writeToConsole)
					OnPrintBufferInfo(name, offset, compValues.Length, GetWramOffset(byteOffset));

				++byteCount;

				if (!writeToConsole) continue;

				string? offsetName = null;
				var tempOffsetName = offsetNameCallback?.Invoke(byteOffset);
				if (tempOffsetName is not null)
					offsetName += $"{tempOffsetName}".PadRight(28, '.');

				if(!isUnknown && offsetName is not null)
					isUnknown = offsetName.ContainsInsensitive(UnknownIdentifier);

				OnPrintComparison(byteOffset, offsetName, currValue, compValue, isUnknown);
			}

			if (byteCount == 0 || !writeToConsole) return byteCount;

			ConsoleHelper.EnsureMinConsoleWidth(ComparisonConsoleWidth);

			OnStatusBytesChanged(byteCount);

			return byteCount;
		}

		protected virtual int? GetWramOffset(int offset) => null;

		protected virtual void OnPrintComparison(int offset, string? offsetName, uint currValue, uint compValue, bool isUnknown) => ConsolePrinter.PrintComparison(" ".Repeat(6), offset, offsetName, currValue, compValue, isUnknown);

		protected virtual void OnPrintComparison(int offset, string? offsetName, ushort currValue, ushort compValue, bool isUnknown) => ConsolePrinter.PrintComparison(" ".Repeat(6), offset, offsetName, currValue, compValue, isUnknown);

		protected virtual void OnPrintComparison(int offset, string? offsetName, byte currValue, byte compValue, bool isUnknown) => ConsolePrinter.PrintComparison(" ".Repeat(6), offset, offsetName, currValue, compValue, isUnknown);

		protected virtual void OnPrintBufferInfo(string name, int offset, int size, int? wramOffset = null) => ConsolePrinter.PrintBufferInfo(name, offset, size, wramOffset);

		protected virtual void OnStatusBytesChanged(int byteCount)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, " ".Repeat(6) + Resources.StatusBytesChangedTemplate.InsertArgs(byteCount));
			ConsolePrinter.ResetColor();
		}
	}
}