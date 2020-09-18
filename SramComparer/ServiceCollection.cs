﻿using SramComparer.Services;

namespace SramComparer
{
    public static class ServiceCollection
    {
        public static ICmdLineParser? CmdLineParser { get; set; }
        public static ICommandExecutor? CommandExecutor { get; set; }

        private static IConsolePrinter? _consolePrinter;
        public static IConsolePrinter ConsolePrinter
        {
            get { return _consolePrinter ??= new ConsolePrinter(); }
            set => _consolePrinter = value;
        }
    }
}