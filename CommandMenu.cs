using System;
using System.IO;
using System.Reflection;
using Common.Shared.Min.Extensions;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;
using SRAM.Comparison.Services;

namespace SRAM.Comparison
{
	/// <summary>Starts a cmd menu loop.</summary>
	public class CommandMenu
	{
		private static CommandMenu? _instance;
		public static CommandMenu Instance => _instance ??= new();
		private static ICommandHandler CommandHandler = ComparisonServices.CommandHandler!;
		private static IConsolePrinter ConsolePrinter = ComparisonServices.ConsolePrinter;

		protected virtual bool? OnRunCommand(ICommandHandler commandHandler, string command, IOptions options) => commandHandler.RunCommand(command!, options);

		public void Show(IOptions options)
		{
			ConsoleHelper.Initialize(options);

			CommandHandler.ThrowIfNull(nameof(CommandHandler));
			ConsolePrinter.ThrowIfNull(nameof(ConsolePrinter));
			options.BatchCommands.ThrowIfNotDefault(nameof(options.BatchCommands));

			ConsolePrinter.PrintSectionHeader();
			var version = ((CommandHandler)CommandHandler).AppVersion!;
			ConsolePrinter.PrintConfigLine("Version", "v" + version);

			if (options.CurrentFilePath.IsNullOrEmpty())
			{
				ConsolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				Console.ReadKey();
				return;
			}

			CheckforUpdates();

			ConsolePrinter.PrintConfig(options);

			if (!File.Exists(FilePathHelper.GetComparisonFilePath(options)))
				CommandHandler.RunCommand(nameof(Commands.OverwriteComp), options);

			ConsolePrinter.PrintStartMessage();
			
			if (options.AutoWatch)
				CommandHandler.RunCommand(nameof(Commands.WatchFile), options);

			while (true)
			{
				try
				{
					var command = Console.ReadLine();

					if (!CommandHandler.RunCommand(command!, options))
						break;
				}
				catch (TargetInvocationException ex)
				{
					ConsolePrinter.PrintError(ex.InnerException!.Message);
					ConsolePrinter.PrintSectionHeader();
				}
				catch (Exception ex)
				{
					ConsolePrinter.PrintError(ex.Message);
					ConsolePrinter.PrintSectionHeader();
				}
			}
		}

		private void CheckforUpdates()
		{
			if (!File.Exists(Services.CommandHandler.DefaultUpdateFileName))
				return;

			((IAutoUpdater)CommandHandler).CheckForUpdates();
		}
	}
}
