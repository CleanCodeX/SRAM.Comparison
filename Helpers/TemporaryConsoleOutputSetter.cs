using System;
using System.IO;

namespace SRAM.Comparison.Helpers
{
	public class TemporaryConsoleOutputSetter : IDisposable
	{
		private readonly TextWriter _oldOut = Console.Out;

		public TemporaryConsoleOutputSetter(TextWriter? output)
		{
			if (output is null) return;

			Console.SetOut(output);
		}

		public void Dispose() => Console.SetOut(_oldOut);
	}
}