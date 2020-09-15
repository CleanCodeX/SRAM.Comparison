using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using App.Commons.Extensions;
using App.Commons.Helpers;
using SramCommons.Models;
using SramComparer.Helpers.Enums;
using SramComparer.Properties;
using static SramComparer.Helpers.ConsolePrinterBase;

namespace SramComparer.Helpers
{
    public abstract class CommandHelperBase<TSramFile, TSramGame>
        where TSramFile : SramFileBase, ISramFile<TSramGame>
        where TSramGame : struct
    {
        public static void CompareFiles<TComparer>(IOptions options)
            where TComparer: ISramComparer<TSramFile, TSramGame>, new()
        {
            if (!File.Exists(options.ComparisonGameFilepath))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Resources.ErrorComparisonFileDoesNotExist, options.ComparisonGameFilepath);
                Console.ResetColor();
                return;
            }

            var currFile = ClassFactory.Create<TSramFile>(options.CurrentGameFilepath, options.Region);
            var compFile = ClassFactory.Create<TSramFile>(options.ComparisonGameFilepath, options.Region);
            var comparer = new TComparer();

            comparer.CompareSram(currFile, compFile, options);

            Console.ResetColor();
        }

        public static void ExportCurrentComparison<TComparer>(IOptions options)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new()
        {
            var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
            var srmFilename = Path.GetFileNameWithoutExtension(options.CurrentGameFilepath);
            var filepath = Path.Join(options.ExportDirectory, $"{srmFilename} # {normalizedTimestamp}.txt");
            var oldOut = Console.Out;

            try
            {
                using var fileStream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
                using var writer = new StreamWriter(fileStream);

                Console.SetOut(writer);

                CompareFiles<TComparer>(options);

                Console.SetOut(oldOut);
                writer.Close();
                fileStream.Close();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Resources.StatusCurrentComparisonExportedFilepathTemplate, filepath);
                Console.ResetColor();

                ExploreFile(filepath);
            }
            catch (Exception ex)
            {
                PrintError(Resources.ErrorCannotOpenOutputFileTemplate.InsertArgs(filepath) + Environment.NewLine + ex.Message);
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

        public static int GetGameId(int maxGameId)
        {
            WriteNewSectionHeader();

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

        public static void OverwriteComparisonFileWithCurrentFile(IOptions options)
        {
            WriteNewSectionHeader();

            File.Copy(options.CurrentGameFilepath!, options.ComparisonGameFilepath!, true);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Resources.StatusCurrentSramFileHasBeenSaved);

            Console.ResetColor();
        }

        public static void BackupSramFile(IOptions options, SramFileKind file, bool restore = false)
        {
            var filepath = file == SramFileKind.Current ? options.CurrentGameFilepath : options.ComparisonGameFilepath;
            var sramName = file == SramFileKind.Current ? Resources.CurrentSramFile : Resources.ComparisonSramFile;

            var directoryPath = Path.GetDirectoryName(filepath);
            var srmFilename = Path.GetFileNameWithoutExtension(filepath);
            var backupFilepath = Path.Join(directoryPath, $"{srmFilename}.backup");

            WriteNewSectionHeader();

            Console.ForegroundColor = ConsoleColor.Yellow;

            if (restore)
            {
                File.Copy(backupFilepath, filepath, true);
                Console.WriteLine(Resources.StatusSramFileHasBeenRestoredFromBackupTemplate, sramName);
            }
            else
            {
                File.Copy(filepath, backupFilepath, true);
                Console.WriteLine(Resources.StatusCurrentSramFileHasBeenBackedUpTemplate, sramName);
            }

            Console.ResetColor();
        }

        public static void TransferSramToOtherGameFile(IOptions options)
        {
            WriteNewSectionHeader();
            var directoryPath = Path.GetDirectoryName(options.CurrentGameFilepath)!;
            var srmFiles = Directory.GetFiles(directoryPath, "*.srm").Where(dp => dp != options.CurrentGameFilepath).ToArray();
            if (srmFiles.Length == 0)
            {
                Console.WriteLine(Resources.StatusNoAvailableOtherSramFiles);
                return;
            }

            var targetFilepath = GetTargetFilepath();
            if (targetFilepath is null)
                return;

            var targetBackupFilepath = targetFilepath + ".backup";
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
                Console.WriteLine(Resources.EnterIndexOfSramFileToBeOverwrittenMaxIndexTemplate, srmFiles.Length);
                Console.WriteLine();
                Console.ResetColor();

                var i = 0;
                foreach (var srmFile in srmFiles)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(i++);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($": {Path.GetFileNameWithoutExtension(srmFile)}");
                }

                var input = Console.ReadLine();

                if (!int.TryParse(input, out var index) || index >= srmFiles.Length)
                {
                    PrintError(Resources.ErrorInvalidIndex);
                    return null;
                }

                return srmFiles[index];
            }
        }

        protected static void InvertIncludeFlag<TEnum>(ref TEnum flags, TEnum flag)
            where TEnum : struct, Enum
        {
            var intFlag = (int)(object)flag;
            var intFlags = (int)(object)flags;

            if (flags.HasFlag(flag))
                intFlags &= ~intFlag;
            else
                intFlags |= intFlag;

            flags = (TEnum)(object)intFlags;

            WriteNewSectionHeader();
            Console.Write(flag + @":");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" " + flags.HasFlag(flag));
            Console.ResetColor();
        }
    }
}