using System;
using System.Drawing;
using System.Runtime.Versioning;

namespace SramComparer.Helpers
{
	[SupportedOSPlatform("windows")]
	public class ConsoleHelper
	{
		private static readonly bool IsWindows = OperatingSystem.IsWindows();
		private const int InitialConsoleWidth = 130;
		private const int InitialConsoleHeight = 50;
		private const int ConsoleBufferHeight = 1000;

		public static void EnsureMinConsoleWidth(int minWidth)
		{
			if (!IsWindows) return;

			try
			{
				if (Console.WindowWidth >= minWidth)
					return;

				Console.BufferWidth = Console.WindowWidth = minWidth;
			}
			catch
			{
				// Ignore
			}
		}

		public static void SetInitialConsoleSize()
		{
			if (!IsWindows) return;

			try
			{
				Console.SetWindowSize(InitialConsoleWidth, InitialConsoleHeight);
				Console.BufferHeight = ConsoleBufferHeight;
				Console.BufferWidth = InitialConsoleWidth;
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
