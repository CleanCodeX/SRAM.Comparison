using System;

namespace SramComparer.Services
{
	/// <summary>Basic console functionality</summary>
	public interface IConsoleMethods
	{
		void PrintParagraph();
		/// <summary>Resets the console's color to default</summary>
		void ResetColor();
		void PrintLine(string text);
		void Print(string text);
		void PrintColored(ConsoleColor color, string text);
		void PrintColoredLine(ConsoleColor color, string text);
		void PrintError(Exception ex);
		void PrintError(string message);
		void PrintFatalError(string fataError);
	}
}