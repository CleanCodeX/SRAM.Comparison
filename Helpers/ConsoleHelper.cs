using System;
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
	}
}
