using SRAM.Comparison.Services;

namespace SRAM.Comparison
{
	public static class ServiceCollection
	{
		public static ICmdLineParser? CmdLineParser { get; set; }
		public static ICommandHandler? CommandHandler { get; set; }

		private static IConsolePrinter? _consolePrinter;
		public static IConsolePrinter ConsolePrinter
		{
			get { return _consolePrinter ??= new ConsolePrinter(); }
			set => _consolePrinter = value;
		}
	}
}