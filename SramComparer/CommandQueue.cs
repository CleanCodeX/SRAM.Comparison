using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Commons.Extensions;
using SramComparer.Properties;
using SramComparer.Services;

// ReSharper disable PossibleMultipleEnumeration

namespace SramComparer
{
    public class CommandQueue
    {
        private static CommandQueue? _instance;
        public static CommandQueue Instance => _instance ??= new CommandQueue();

        protected virtual bool? OnRunCommand(ICommandHandler commandHandler, string command, IOptions options) => commandHandler.RunCommand(command!, options);

        public void Run(IOptions options)
        {
            var commandExecutor = ServiceCollection.CommandHandler;
            var consolePrinter = ServiceCollection.ConsolePrinter;

            commandExecutor.ThrowIfNull(nameof(commandExecutor));
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

                    if (OnRunCommand(commandExecutor, command!, options) == false)
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
