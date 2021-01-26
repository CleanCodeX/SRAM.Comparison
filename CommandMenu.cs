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

		protected virtual bool? OnRunCommand(ICommandHandler commandHandler, string command, IOptions options) => commandHandler.RunCommand(command!, options);

		public virtual void Show(IOptions options)
		{
			ConsoleHelper.Initialize(options);

			var commandHandler = ServiceCollection.CommandHandler;
			var consolePrinter = ServiceCollection.ConsolePrinter;

			commandHandler.ThrowIfNull(nameof(commandHandler));
			consolePrinter.ThrowIfNull(nameof(consolePrinter));
			options.BatchCommands.ThrowIfNotDefault(nameof(options.BatchCommands));

			if (options.CurrentFilePath.IsNullOrEmpty())
			{
				consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				Console.ReadKey();
				return;
			}

			consolePrinter.PrintConfig(options);

			if (!File.Exists(FilePathHelper.GetComparisonFilePath(options)))
				commandHandler.RunCommand(nameof(Commands.OverwriteComp), options);

			consolePrinter.PrintStartMessage();

			while (true)
			{
				try
				{
					var command = Console.ReadLine();

					if (!commandHandler.RunCommand(command!, options))
						break;
				}
				catch (TargetInvocationException ex)
				{
					consolePrinter.PrintError(ex.InnerException!.Message);
					consolePrinter.PrintSectionHeader();
				}
				catch (Exception ex)
				{
					consolePrinter.PrintError(ex.Message);
					consolePrinter.PrintSectionHeader();
				}
			}
		}
	}
}
