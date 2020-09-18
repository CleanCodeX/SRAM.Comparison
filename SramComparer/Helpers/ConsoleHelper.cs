using System;

namespace SramComparer.Helpers
{
    public static class ConsoleHelper
    {
        public static void EnsureMinConsoleWidth(int minWidth)
        {
            if (!OperatingSystem.IsWindows() || Console.WindowWidth >= minWidth)
                return;

            try
            {
                Console.WindowWidth = minWidth;
            }
            catch
            {
                // Ignore
            }
        }

        public static void SetInitialConsoleSize()
        {
            if (!OperatingSystem.IsWindows()) return;

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
