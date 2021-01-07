using System;
using System.Drawing;
using System.Runtime.Versioning;

namespace SramComparer.Helpers
{
	[SupportedOSPlatform("windows")]
	public class ConsoleHelper
	{
		private static readonly bool IsWindows = OperatingSystem.IsWindows();

		public static void EnsureMinConsoleWidth(int minWidth)
		{
			if (!IsWindows) return;

			try
			{
				if (Console.WindowWidth >= minWidth)
					return;

				Console.WindowWidth = minWidth;
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
				Console.SetWindowSize(130, 50);
				Console.BufferHeight = 1000;
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
