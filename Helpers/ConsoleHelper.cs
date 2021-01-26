using System;
using System.Drawing;
using System.Runtime.Versioning;
using SRAM.Comparison.Services;

namespace SRAM.Comparison.Helpers
{
	[SupportedOSPlatform("windows")]
	public class ConsoleHelper
	{
		private const int InitialConsoleWidth = 130;
		private const int InitialConsoleHeight = 50;
		private const int ConsoleBufferHeight = 1000;

		private static readonly IConsolePrinter ConsolePrinter = ServiceCollection.ConsolePrinter;
		private static readonly bool IsWindows = OperatingSystem.IsWindows();
		
		public static void EnsureMinConsoleWidth(int minWidth)
		{
			if (!IsWindows) return;

			try
			{
				if (Console.WindowWidth >= minWidth && Console.BufferWidth >= minWidth)
					return;

				Console.BufferWidth = Console.WindowWidth = minWidth;
			}
			catch
			{
				// Ignore
			}
		}

		public static void Initialize(IOptions options)
		{
			if (options.UILanguage is not null)
				CultureHelper.TrySetCulture(options.UILanguage, ConsolePrinter);

			ConsolePrinter.ColorizeOutput = options.ColorizeOutput;

			SetInitialConsoleSize();
		}

		public static void SetInitialConsoleSize()
		{
			if (!IsWindows) return;

			try
			{
				if (Console.WindowWidth > InitialConsoleWidth) return;

				Console.SetWindowSize(InitialConsoleWidth, InitialConsoleHeight);
				Console.SetBufferSize(InitialConsoleWidth, ConsoleBufferHeight);
			}
			catch
			{
				// Ignore
			}
		}

		public static void RedefineConsoleColors(Color color = default, Color bgColor = default)
		{
			if (!IsWindows) return;

			try
			{
				if (color == default && bgColor == default) return;

				if (color == default) color = Color.FromName(nameof(ConsoleColor.Gray));
				if (bgColor == default) bgColor = Color.FromName(nameof(ConsoleColor.Black));

				PaletteHelper.SetScreenColors(color, bgColor);

				Console.Clear();
			}
			catch
			{
				// Ignore
			}
		}
	}
}
