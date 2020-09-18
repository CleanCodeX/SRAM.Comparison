using System;
using System.IO;
using App.Commons.Extensions;
using SramComparer.Helpers;
using SramComparer.Properties;
using SramComparer.Services;

namespace SramComparer
{
    public class CommandMenu
    {
        private static CommandMenu? _instance;
        public static CommandMenu Instance => _instance ??= new CommandMenu();

        protected virtual bool? OnRunCommand(ICommandExecutor commandExecutor, string command, IOptions options) => commandExecutor.RunCommand(command!, options);

        public virtual void Run(IOptions options)
        {
            var commandExecutor = ServiceCollection.CommandExecutor;
            var consolePrinter = ServiceCollection.ConsolePrinter;

            commandExecutor.ThrowIfNull(nameof(commandExecutor));
            consolePrinter.ThrowIfNull(nameof(consolePrinter));
            options.Commands.ThrowIfNotDefault(nameof(options.Commands));

            if (options.CurrentGameFilepath.IsNullOrEmpty())
            {
                consolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
                return;
            }

            ConsoleHelper.SetInitialConsoleSize();
            consolePrinter.PrintSettings(options);
            consolePrinter.PrintCommands();

            while (true)
            {
                try
                {
                    consolePrinter.PrintSectionHeader();
                    var command = Console.ReadLine();

                    if (OnRunCommand(commandExecutor, command!, options) == false)
                        break;
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
