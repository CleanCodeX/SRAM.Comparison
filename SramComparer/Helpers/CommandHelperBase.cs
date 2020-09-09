using System;
using System.Diagnostics;
using System.IO;
using App.Commons.Extensions;
using App.Commons.Helpers;
using SramCommons.Models;
using SramComparer.Helpers.Enums;
using SramComparer.Properties;
using static SramComparer.Helpers.ConsolePrinterBase;

namespace SramComparer.Helpers
{
    public abstract class CommandHelperBase<TSramFile, TSramGame, TGameId>
        where TSramFile : SramFileBase, ISramFile<TSramGame, TGameId>
        where TSramGame : struct
        where TGameId : struct, Enum
    {
        public static void CompareFiles<TComparer>(IOptions options)
            where TComparer: ISramComparer<TSramFile, TSramGame, TGameId>, new()
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
            where TComparer : ISramComparer<TSramFile, TSramGame, TGameId>, new()
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

        protected static void InvertIncludeFlag<TEnum>(ref TEnum flags, TEnum flag)
            where TEnum: struct, Enum
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

        public static TGameId GetGameId()
        {
            WriteNewSectionHeader();

#pragma warning disable 8631
            var maxValue = default(TGameId).GetMaxValue();
#pragma warning restore 8631
            Console.WriteLine(Resources.SetGameMaxTemplate.InsertArgs(maxValue));

            var input = Console.ReadLine()!;
            var gameId = input.ParseEnum<TGameId>();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(!Equals(gameId, default(TGameId))
                ? string.Format(Resources.StatusGameWillBeComparedTemplate, gameId.ToInt())
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
            var filepath = file == SramFileKind.Curr ? options.CurrentGameFilepath : options.ComparisonGameFilepath;
            var sramName = file == SramFileKind.Curr ? Resources.CurrentSramFile : Resources.ComparisonSramFile;

            var directoryPath = Path.GetDirectoryName((string?) filepath);
            var srmFilename = Path.GetFileNameWithoutExtension((string?) filepath);
            var backupFilepath = Path.Join(directoryPath, $"{srmFilename} ### {Resources.Backup}.srm");

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
    }
}