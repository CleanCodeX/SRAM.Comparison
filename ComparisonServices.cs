using System.Runtime.CompilerServices;
using IO;
using IO.Modules.Services;
using SRAM.Comparison.Services;

namespace SRAM.Comparison
{
	public static class ComparisonServices
	{
		public static ICmdLineParser? CmdLineParser { get; set; }
		public static ICommandHandler? CommandHandler { get; set; }

		private static IConsolePrinter? _consolePrinter;
		public static IConsolePrinter ConsolePrinter
		{
			get { return _consolePrinter ??= new ConsolePrinter(); }
			set => _consolePrinter = value;
		}

		[ModuleInitializer]
		public static void InitializeServices()
		{
			IOServices.ArrayFormatter = new ConsoleArrayFormatter();
			IOServices.StructFormatter = new ConsoleStructFormatter();
		}
	}
}