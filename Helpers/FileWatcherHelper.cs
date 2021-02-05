using System;
using System.IO;
using System.Threading;
using Common.Shared.Min.Extensions;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Properties;
using SRAM.Comparison.Services;

namespace SRAM.Comparison.Helpers
{
	internal static class FileWatcherHelper
	{
		private const int ProcessWaitMiliseconds = 50;
		private static ICommandHandler CommandHandler => ComparisonServices.CommandHandler!;
		private static IConsolePrinter ConsolePrinter => ComparisonServices.ConsolePrinter;
		private static DateTime lastReadTime ;
		private static FileSystemWatcher? _fileSystemWatcher;

		public static void StopWatching(IOptions options)
		{
			_fileSystemWatcher?.Dispose();
			_fileSystemWatcher = null;
			PrintWatchingStopped(options);
		}

		public static void StartWatching(IOptions options)
		{
			var filePath = options.CurrentFilePath!;
			var directory = Path.GetDirectoryName(filePath)!;
			var fileName = Path.GetFileName(filePath)!;

			_fileSystemWatcher = new(directory, fileName)
			{
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.LastWrite
			};

			_fileSystemWatcher.Changed += (_, _) => OnFileChanged(options);

			OverwriteComp(options);
			PrintWatchingStarted(options);
		}

		private static bool IsFileChange(string filePath)
		{
			var lastWriteTime = File.GetLastWriteTime(filePath);
			if ((lastWriteTime - lastReadTime).TotalSeconds <= 1) return false;

			lastReadTime = lastWriteTime;
			return true;
		}

		private static void OnFileChanged(IOptions options)
		{
			if (!IsFileChange(options.CurrentFilePath!)) return;

			PrintFileChanged();

			Thread.Sleep(ProcessWaitMiliseconds);

			try
			{
				// Lock file for writing
				using (File.Open(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read))
					Compare(options);
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintFatalError(ex.Message);
			}
		}

		private static void Compare(IOptions options) => RunCommand(nameof(Commands.Compare), options);
		private static void OverwriteComp(IOptions options) => RunCommand(nameof(Commands.OverwriteComp), options);

		private static bool RunCommand(string command, IOptions options) => CommandHandler.RunCommand(command, options);

		private static void PrintWatchingStarted(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusFileWatchingStartedTemplate.InsertArgs(options.CurrentFilePath));
			ConsolePrinter.ResetColor();
		}

		private static void PrintWatchingStopped(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusFileWatchingStoppedTemplate.InsertArgs(options.CurrentFilePath));
			ConsolePrinter.ResetColor();
		}

		private static void PrintFileChanged()
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, Resources.StatusWatchedFileChanged);
		}
	}
}