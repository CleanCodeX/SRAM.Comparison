using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using App.Commons.Extensions;
using App.Commons.Helpers;
using SramCommons.Models;
using SramComparer.Enums;
using SramComparer.Helpers;
using SramComparer.Properties;
// ReSharper disable RedundantArgumentDefaultValue

namespace SramComparer.Services
{
    public class CommandHandler<TSramFile, TSramGame> : ICommandHandler<TSramFile, TSramGame>
        where TSramFile : SramFile, ISramFile<TSramGame>
        where TSramGame : struct
    {
        private const string BackupFileExtension = ".backup";

        private IConsolePrinter ConsolePrinter { get; }

        public CommandHandler() : this(ServiceCollection.ConsolePrinter) {}
        public CommandHandler(IConsolePrinter consolePrinter) => ConsolePrinter = consolePrinter;

        public virtual bool RunCommand(string command, IOptions options, TextWriter? outStream = null)
        {
            ConsoleHelper.SetInitialConsoleSize();

            var defaultStream = Console.Out;

            if (outStream is not null)
                Console.SetOut(outStream);

            if (options.CurrentGameFilepath.IsNullOrEmpty())
            {
                ConsolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
                return false;
            }

            try
            {
                return OnRunCommand(command, options);
            }
            finally
            {
                if (outStream is not null)
                    Console.SetOut(defaultStream);
            }
        }

        protected internal virtual bool OnRunCommand(string command, IOptions options)
        {
            Requires.NotNull(command, nameof(command));
            Requires.NotNull(options, nameof(options));

            switch (command)
            {
                case "":
                    break;
                case nameof(BaseCommands.c):
                case nameof(BaseCommands.e):
                    throw new NotImplementedException(Resources.ErrorCommandNotImplementedTemplate.InsertArgs(command));
                case "help":
                case "?":
                case nameof(BaseCommands.cmd):
                    ConsolePrinter.PrintCommands();
                    break;
                case nameof(BaseCommands.s):
                    ConsolePrinter.PrintSettings(options);
                    break;
                case nameof(BaseCommands.m):
                    ConsolePrinter.PrintManual();
                    break;
                case nameof(BaseCommands.fwg):
                    options.Flags = InvertIncludeFlag(options.Flags, ComparisonFlags.WholeGameBuffer);
                    break;
                case nameof(BaseCommands.fng):
                    options.Flags = InvertIncludeFlag(options.Flags, ComparisonFlags.NonGameBuffer);
                    break;
                case nameof(BaseCommands.sg):
                    options.Game = GetGameId(maxGameId: 4);
                    if (options.Game == default)
                        options.ComparisonGame = default;

                    break;
                case nameof(BaseCommands.sgc):
                    if (options.Game != default)
                        options.ComparisonGame = GetGameId(maxGameId: 4);
                    else
                        ConsolePrinter.PrintError(Resources.ErrorComparisoGameSetButNotGame);

                    break;
                case nameof(BaseCommands.ow):
                    OverwriteComparisonFileWithCurrentFile(options);
                    break;
                case nameof(BaseCommands.b):
                    BackupSramFile(options, SramFileKind.Current, false);
                    break;
                case nameof(BaseCommands.r):
                    BackupSramFile(options, SramFileKind.Current, true);
                    break;
                case nameof(BaseCommands.bc):
                    BackupSramFile(options, SramFileKind.Comparison, false);
                    break;
                case nameof(BaseCommands.rc):
                    BackupSramFile(options, SramFileKind.Comparison, true);
                    break;
                case nameof(BaseCommands.ts):
                    TransferSramToOtherGameFile(options);
                    break;
                case nameof(BaseCommands.w):
                    Console.Clear();
                    break;
                case nameof(BaseCommands.q):
                    return false;
                default:
                    ConsolePrinter.PrintCommands();
                    ConsolePrinter.PrintError(Resources.ErrorNoValidCommand.InsertArgs(command));

                    break;
            }

            return true;
        }

        public virtual void Compare<TComparer>(IOptions options)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            Requires.FileExists(options.ComparisonGameFilepath, nameof(options.ComparisonGameFilepath), Resources.ErrorComparisonFileDoesNotExist);

            var currFileStream = new FileStream(options.CurrentGameFilepath, FileMode.Open, FileAccess.Read);
            var compFileStream = new FileStream(options.ComparisonGameFilepath, FileMode.Open, FileAccess.Read);

            Compare<TComparer>(currFileStream, compFileStream, options);
        }

        public void Compare<TComparer>(IOptions options, TextWriter output) 
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            var oldOut = Console.Out;
            Console.SetOut(output);

            Compare<TComparer>(options);

            Console.SetOut(oldOut);
            Console.ResetColor();
        }

        public virtual void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options, TextWriter output)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            var oldOut = Console.Out;
            Console.SetOut(output);

            Compare<TComparer>(currStream, compStream, options);

            Console.SetOut(oldOut);
        }

        public virtual void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            var currFile = ClassFactory.Create<TSramFile>(currStream, options.Region);
            var compFile = ClassFactory.Create<TSramFile>(compStream, options.Region);
            var comparer = new TComparer();

            comparer.CompareSram(currFile, compFile, options);

            Console.ResetColor();
        }

        public virtual void ExportComparison<TComparer>(IOptions options, bool showInExplorer = false)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
            var srmFilename = Path.GetFileNameWithoutExtension(options.CurrentGameFilepath);
            var filepath = Path.Join(options.ExportDirectory, $"{srmFilename} # {normalizedTimestamp}.txt");

            ExportComparison<TComparer>(options, filepath, showInExplorer);
        }

        public virtual void ExportComparison<TComparer>(IOptions options, string filepath, bool showInExplorer = false)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            try
            {
                using var fileStream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
                using var writer = new StreamWriter(fileStream);

                Compare<TComparer>(options, new StreamWriter(fileStream));

                writer.Close();
                fileStream.Close();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Resources.StatusCurrentComparisonExportedFilepathTemplate, filepath);

                if(showInExplorer)
                    ExploreFile(filepath);
            }
            catch (Exception ex)
            {
                ConsolePrinter.PrintError(Resources.ErrorCannotOpenOutputFileTemplate.InsertArgs(filepath) +
                                          Environment.NewLine + ex.Message);
            }

            Console.ResetColor();

            static void ExploreFile(string filePath)
            {
                if (!File.Exists(filePath))
                    return;

                //Clean up file path so it can be navigated OK
                filePath = Path.GetFullPath(filePath);
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }

        public virtual void TransferSramToOtherGameFile(IOptions options)
        {
            ConsolePrinter.PrintSectionHeader();
            var directoryPath = Path.GetDirectoryName(options.CurrentGameFilepath)!;
            var srmFiles = Directory.GetFiles(directoryPath, "*.srm").Where(f => f != options.CurrentGameFilepath).ToArray();
            if (srmFiles.Length == 0)
            {
                Console.WriteLine(Resources.StatusNoAvailableOtherSramFiles);
                return;
            }

            var targetFilepath = GetTargetFilepath();
            if (targetFilepath is null)
                return;

            var targetBackupFilepath = targetFilepath + BackupFileExtension;
            if (!File.Exists(targetBackupFilepath))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                File.Copy(targetFilepath, targetBackupFilepath);
                Console.WriteLine(Resources.StatusTargetSramFileHasBeenBackedUpFilepathTemplate, Path.GetFileName(targetBackupFilepath));
            }

            File.Copy(options.CurrentGameFilepath, targetFilepath, true);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Resources.StatusCurrentSramHasBeenSavedAsFilepathTemplate, Path.GetFileName(targetFilepath));

            string? GetTargetFilepath()
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Resources.EnterIndexOfSramFileToBeOverwrittenMaxIndexTemplate, srmFiles.Length - 1);
                Console.WriteLine();
                Console.ResetColor();

                var i = 0;
                foreach (var srmFile in srmFiles)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(i++);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($@": {Path.GetFileNameWithoutExtension(srmFile)}");
                }

                var input = Console.ReadLine();

                if (!int.TryParse(input, out var index) || index >= srmFiles.Length)
                {
                    ConsolePrinter.PrintError(Resources.ErrorInvalidIndex);
                    return null;
                }

                return srmFiles[index];
            }
        }

        public Enum InvertIncludeFlag(Enum flags, Enum flag)
        {
            var intFlag = flag.ToUInt();
            var intFlags = flags.ToUInt();

            var enumType = flags.GetType();
            var enumFlag = (Enum)Enum.ToObject(enumType, intFlag);

            if (flags.HasFlag(enumFlag))
                intFlags &= ~intFlag;
            else
                intFlags |= intFlag;

            flags = (Enum)Enum.ToObject(enumType, intFlags);

            ConsolePrinter.PrintInvertIncludeFlag(flags, enumFlag);

            return flags;
        }

        public virtual int GetGameId(int maxGameId)
        {
            ConsolePrinter.PrintSectionHeader();

            Console.WriteLine(Resources.SetGameMaxTemplate.InsertArgs(maxGameId));

            var input = Console.ReadLine()!;

            int.TryParse(input, out var gameId);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(gameId == 0
                ? string.Format(Resources.StatusGameWillBeComparedTemplate, gameId)
                : Resources.StatusAllGamesWillBeCompared);

            Console.WriteLine();
            Console.ResetColor();

            return gameId;
        }

        public virtual void OverwriteComparisonFileWithCurrentFile(IOptions options)
        {
            ConsolePrinter.PrintSectionHeader();

            File.Copy(options.CurrentGameFilepath!, options.ComparisonGameFilepath!, true);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Resources.StatusCurrentSramFileHasBeenSaved);

            Console.ResetColor();
        }

        public virtual void BackupSramFile(IOptions options, SramFileKind file, bool restore = false)
        {
            var filepath = file == SramFileKind.Current ? options.CurrentGameFilepath : options.ComparisonGameFilepath;
            var fileTypeName = file == SramFileKind.Current ? Resources.CurrentSramFile : Resources.ComparisonSramFile;
            var backupFilepath = filepath += BackupFileExtension;

            ConsolePrinter.PrintSectionHeader();

            Console.ForegroundColor = ConsoleColor.Yellow;

            if (restore)
            {
                File.Copy(backupFilepath, filepath, true);
                Console.WriteLine(Resources.StatusSramFileHasBeenRestoredFromBackupTemplate, fileTypeName);
            }
            else
            {
                File.Copy(filepath, backupFilepath, true);
                Console.WriteLine(Resources.StatusCurrentSramFileHasBeenBackedUpTemplate, fileTypeName);
            }

            Console.ResetColor();
        }
    }
}
