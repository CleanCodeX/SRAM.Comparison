using System;

namespace SramComparer.Services
{
	public interface IConsoleMethods
	{
		void PrintParagraph();
		void ResetColor();
		void PrintLine(object text);
		void Print(object text);
		void PrintColored(ConsoleColor color, object text);
		void PrintColoredLine(ConsoleColor color, object text);
		void PrintError(Exception ex);
		void PrintError(string message);
		void PrintFatalError(string fataError);
	}

	public interface IConsolePrinter: IConsoleMethods
	{
		void PrintSectionHeader();
		
		void PrintStartMessage();

		void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength);

		void PrintComparison(string ident, int offset, string? offsetName, ushort currValue, ushort compValue);
		void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue);

		void PrintInvertIncludeFlag(Enum flags, Enum flag);

		void PrintManual();
		void PrintCommands();
		void PrintSettings(IOptions options);
	}
}