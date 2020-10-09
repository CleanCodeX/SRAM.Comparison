using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using SramComparer.Properties;

// ReSharper disable PossibleMultipleEnumeration

namespace SramComparer
{
	/// <summary>Starts a UI-less cmd queue.</summary>
	public class CommandQueue
	{
		private static CommandQueue? _instance;
		public static CommandQueue Instance => _instance ??= new CommandQueue();

		public void Start(IOptions options)
		{
			var commandHandler = ServiceCollection.CommandHandler;
			var consolePrinter = ServiceCollection.ConsolePrinter;

			commandHandler.ThrowIfNull(nameof(commandHandler));
			consolePrinter.ThrowIfNull(nameof(consolePrinter));
			options.Commands.ThrowIfNull(nameof(options.Commands));

			if (options.CurrentGameFilepath.IsNullOrEmpty())
			{
				consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				return;
			}

			var commands = options.Commands?.Split('-') ?? Enumerable.Empty<string>();
			var queuedCommands = new Queue<string>(commands);

			Console.WriteLine(@$"{Resources.QueuedCommands}: {queuedCommands.Count} ({string.Join(", ", commands)})");

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
