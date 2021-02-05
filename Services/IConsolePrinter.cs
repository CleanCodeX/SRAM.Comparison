using System;

namespace SRAM.Comparison.Services
{
	/// <summary>Interface for <see cref="ConsolePrinter"/> implementations</summary>
	public interface IConsolePrinter: IConsoleMethods
	{
		bool ColorizeOutput { get; set; }
		string NewLine => Environment.NewLine;

		/// <summary>Prints a section's header</summary>
		void PrintSectionHeader();
		
		/// <summary>Prints a special message only when the app starts</summary>
		void PrintStartMessage();

		/// <summary>Prints info about the compared buffer</summary>
		/// <param name="bufferName">The buffer's name</param>
		/// <param name="bufferOffset">The buffer's offset</param>
		/// <param name="bufferLength">The buffer's length</param>
		void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength);

		/// <summary>Prints the comparison result</summary>
		/// <param name="ident">An indentification string to use</param>
		/// <param name="offset">The values's offset</param>
		/// /// <param name="offsetName">The values's name</param>
		/// <param name="currValue">The current value for comparison</param>
		/// <param name="compValue">The comparison value for comparison</param>
		/// <param name="isUnknown">Indicates that this offset is considered to be unknown</param>
		void PrintComparison(string ident, int offset, string? offsetName, ushort currValue, ushort compValue, bool isUnknown);

		/// <inheritdoc cref="PrintComparison(string, int, string, ushort, ushort, bool)"/>
		void PrintComparison(string ident, int offset, string? offsetName, uint currValue, uint compValue, bool isUnknown);

		/// <inheritdoc cref="PrintComparison(string, int, string, ushort, ushort, bool)"/>
		void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue, bool isUnknown);

		/// <summary>
		/// Prints the inverted status of a flag
		/// </summary>
		/// <param name="flags">The flags which the flag will be added to or removd from</param>
		/// <param name="flag">The flag to be inverted</param>
		void PrintInvertIncludeFlag(in Enum flags, in Enum flag);

		/// <summary>
		/// Prints the the set flags
		/// </summary>
		/// <param name="flags">The flags which are set</param>
		/// <param name="name">The name of the flags</param>
		void PrintFlags(in Enum flags, string? name = null);

		/// <summary>Prints a manual for how to use the program</summary>
		void PrintGuide(string guideName);

		/// <summary>Prints a list of available commands</summary>
		void PrintCommands();

		/// <summary>Prints the current settings</summary>
		void PrintConfig(IOptions options);

		/// <summary>Prints a line with config style name and value.</summary>
		/// <param name="name">The config's name to print.</param>
		/// <param name="value">The config's value to print. Appears after the ':'.</param>
		void PrintConfigLine(string name, string value);

		/// <summary>Prints a line with config style name and value.</summary>
		/// <param name="name">The config's name to print.</param>
		/// <param name="args">The config's arguments to print. Appears before the ':'</param>
		/// <param name="value">The config's value to print. Appears after the ':'.</param>
		void PrintConfigLine(string name, string args, string value);

		/// <summary>Clears the output window</summary>
		void Clear();

		/// <summary>Sets the foreground color</summary>
		/// <param name="color">The color to set</param>
		void SetForegroundColor(ConsoleColor color);

		/// <summary>Sets the background color</summary>
		/// <param name="color">The color to set</param>
		void SetBackgroundColor(ConsoleColor color);
	}
}