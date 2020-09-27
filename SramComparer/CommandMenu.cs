using System;
using System.IO;
using System.Reflection;
using App.Commons.Extensions;
using SramCommons.Exceptions;
using SramComparer.Helpers;
using SramComparer.Properties;
using SramComparer.Services;

namespace SramComparer
{
    public class CommandMenu
    {
        private static CommandMenu? _instance;
        public static CommandMenu Instance => _instance ??= new CommandMenu();

        protected virtual bool? OnRunCommand(ICommandHandler commandHandler, string command, IOptions options) => commandHandler.RunCommand(command!, options);

        public virtual void Run(IOptions options)
        {
            var commandHandler = ServiceCollection.CommandHandler;
            var consolePrinter = ServiceCollection.ConsolePrinter;

            commandHandler.ThrowIfNull(nameof(commandHandler));
            consolePrinter.ThrowIfNull(nameof(consolePrinter));
            options.Commands.ThrowIfNotDefault(nameof(options.Commands));

            if (options.CurrentGameFilepath.IsNullOrEmpty())
            {
                consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
                return;
            }

            ConsoleHelper.SetInitialConsoleSize();
            consolePrinter.PrintSettings(options);
            consolePrinter.PrintStartMessage();

            while (true)
            {
                try
                {
                    //consolePrinter.PrintSectionHeader();
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
