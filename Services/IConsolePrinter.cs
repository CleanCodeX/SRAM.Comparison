using System;

namespace SramComparer.Services
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
		void PrintComparison(string ident, int offset, string? offsetName, ushort currValue, ushort compValue);

		/// <inheritdoc cref="PrintComparison(string, int, string, ushort, ushort)"/>
		void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue);

		/// <summary>
		/// Prints the inverted status of a flag
		/// </summary>
		/// <param name="flags">The flags which the flag will be added to or removd from</param>
		/// <param name="flag">The flag to be inverted</param>
		void PrintInvertIncludeFlag(Enum flags, Enum flag);

		/// <summary>Prints a manual for how to use the program</summary>
		void PrintManual();

		/// <summary>Prints a list of available commands</summary>
		void PrintCommands();

		/// <summary>Prints the current settings</summary>
		void PrintSettings(IOptions options);
	}
}