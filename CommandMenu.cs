using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using Common.Shared.Min.Extensions;
using SramComparer.Helpers;
using SramComparer.Properties;
using SramComparer.Services;

namespace SramComparer
{
	/// <summary>Starts a cmd menu loop.</summary>
	public class CommandMenu
	{
		private static CommandMenu? _instance;
		public static CommandMenu Instance => _instance ??= new();

		protected virtual bool? OnRunCommand(ICommandHandler commandHandler, string command, IOptions options) => commandHandler.RunCommand(command!, options);

		public virtual void Show(IOptions options)
		{
			var commandHandler = ServiceCollection.CommandHandler;
			var consolePrinter = ServiceCollection.ConsolePrinter;

			commandHandler.ThrowIfNull(nameof(commandHandler));
			consolePrinter.ThrowIfNull(nameof(consolePrinter));
			options.BatchCommands.ThrowIfNotDefault(nameof(options.BatchCommands));

			if(options.UILanguage is not null)
				TrySetCulture(consolePrinter, options.UILanguage);

			if (options.CurrentFilePath.IsNullOrEmpty())
			{
				consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				Console.ReadKey();
				return;
			}

			ConsoleHelper.SetInitialConsoleSize();
			ConsoleHelper.RedefineConsoleColors(bgColor: Color.FromArgb(17, 17, 17));

			consolePrinter.PrintConfig(options);
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

		private static void TrySetCulture(IConsolePrinter consolePrinter, string culture)
		{
			try
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
			}
			catch (Exception ex)
			{
				consolePrinter.PrintError(ex.Message);
				consolePrinter.PrintSectionHeader();
			}
		}
	}
}
