using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Commons.Extensions;
using SramComparer.Properties;

// ReSharper disable PossibleMultipleEnumeration

namespace SramComparer
{
	public class CommandQueue
	{
		private static CommandQueue? _instance;
		public static CommandQueue Instance => _instance ??= new CommandQueue();

		public void Run(IOptions options)
		{
			var commandHandler = ServiceCollection.CommandHandler;
			var consolePrinter = ServiceCollection.ConsolePrinter;

			commandHandler.ThrowIfNull(nameof(commandHandler));
			consolePrinter.ThrowIfNull(nameof(consolePrinter));
			options.Commands.ThrowIfNull(nameof(options.Commands));

			if (options.CurrentGameFilepath.IsNullOrEmpty())
			{
				consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				Console.ReadKey();
				return;
			}

			var commands = options.Commands?.Split('-') ?? Enumerable.Empty<string>();
			var queuedCommands = new Queue<string>(commands);

			consolePrinter.PrintLine(@$"{Resources.QueuedCommands}: {queuedCommands.Count} ({string.Join(", ", commands)})");

			while (true)
			{
				try
				{
					string? command = null;
					if (queuedCommands.Count > 0)
						queuedCommands.TryDequeue(out command);

					if (commandHandler.RunCommand(command!, options) == false)
						break;

#if !DEBUG
					if (queuedCommands.Count == 0)
						break;
#endif
				}
				catch (IOException ex)
				{
					consolePrinter.PrintError(ex.Message);
					consolePrinter.PrintSectionHeader();
				}
				catch (Exception ex)
				{
					consolePrinter.PrintError(ex);
					consolePrinter.PrintSectionHeader();
				}
			}
		}
	}
}
