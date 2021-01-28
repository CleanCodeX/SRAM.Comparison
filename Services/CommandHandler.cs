using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using Common.Shared.Min.Helpers;
using IO.Extensions;
using IO.Models;
using ObjectExtensions.Copy;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable StaticMemberInGenericType

namespace SRAM.Comparison.Services
{
	public abstract class CommandHandler
	{
		protected const string DefaultConfigName = "Config";
		protected const string DefaultLogFileName = "Log.txt";

		public static readonly string KeyBindingsFileName = "KeyBindings.json";
		public static readonly string DefaultConfigFileName = $"{DefaultConfigName}.json";
	}

	/// <summary>
	/// This class handles all standard commands
	/// </summary>
	/// <typeparam name="TSramFile">The S-RAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The S-RAM game structure</typeparam>
	public abstract class CommandHandler<TSramFile, TSaveSlot> : CommandHandler, ICommandHandler<TSramFile, TSaveSlot>
		where TSramFile : class, IMultiSegmentFile<TSaveSlot>, IRawSave
		where TSaveSlot : struct
	{
		private const string BackupFileExtension = ".backup";
		private const string SrmFileExtension = ".srm";
		private const string CompFileExtension = ".comp";
		
		private const string GuideSrmFileName = "guide-srm";
		private const string GuideSavestateFileName = "guide-savestate";

		protected IConsolePrinter ConsolePrinter { get; }

		#region Ctors

		public CommandHandler() : this(ServiceCollection.ConsolePrinter) {}
		/// <param name="consolePrinter">A specific console printer instance</param>
		public CommandHandler(IConsolePrinter consolePrinter) => ConsolePrinter = consolePrinter;

		#endregion Ctors

		#region Command Handling

		/// <summary>Runs a specific command</summary>
		/// <param name="command">The command to be run</param>
		/// <param name="options">The options to use for the command</param>
		/// <param name="output">The optionl stream the output should be written to if not to standard console</param>
		/// <returns>False if the game command loop should exit, otherwise true</returns>
		public virtual bool RunCommand(string command, IOptions options, TextWriter? output = null)
		{
			ConsoleHelper.Initialize(options);

			using (new TemporaryConsoleOutputSetter(output))
			{
				if (options.CurrentFilePath.IsNullOrEmpty())
				{
					ConsolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
					Console.ReadKey();
					return true;
				}

				return OnRunCommand(command, options);
			}
		}

		/// <summary>Allows to overwrite control default handling for commands</summary>
		/// <param name="command">The command to be run</param>
		/// <param name="options">The options to be used for the command</param>
		/// <returns>False if the game command loop should exit, otherwise true</returns>
		protected internal virtual bool OnRunCommand(string command, IOptions options)
		{
			Requires.NotNull(command, nameof(command));
			Requires.NotNull(options, nameof(options));

			if (command.IsNullOrEmpty()) return true;
			if (command == "?") command = nameof(Commands.Help);

			var cmd = command.ParseEnum<Commands>();
			if (cmd == default)
			{
				if (Enum.TryParse<AlternateCommands>(command, true, out var altCommand))
					command = ((Commands)altCommand).ToString();
				else if (CheckCustomKeyBinding(command, out var boundCommand))
					command = boundCommand;

				cmd = command.ParseEnum<Commands>();
			}

			switch (cmd)
			{
				case Commands.Compare:
				case Commands.Export:
					throw new NotImplementedException(Resources.ErrorCommandNotImplementedTemplate.InsertArgs(command));
				case Commands.ComparisonFlags:
					SetComparisonFlags(options);
					CheckAutoSave(options);
					break;
				case Commands.ExportFlags:
					SetExportFlags(options);
					CheckAutoSave(options);
					break;
				case Commands.FileWatchFlags:
					SetFileWatchFlags(options);
					CheckAutoSave(options);
					break;
				case Commands.LogFlags:
					SetLogFlags(options);
					CheckAutoSave(options);
					break;
				case Commands.Help:
					ConsolePrinter.PrintCommands();
					break;
				case Commands.Config:
					ConsolePrinter.PrintConfig(options);
					break;
				case Commands.SrmGuide:
					ConsolePrinter.PrintGuide(GuideSrmFileName);
					break;
				case Commands.SavestateGuide:
					ConsolePrinter.PrintGuide(GuideSavestateFileName);
					break;
				case Commands.ChecksumStatus:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.ChecksumStatus);
					CheckAutoSave(options);
					break;
				case Commands.SlotByteComp:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.SlotByteComparison);
					CheckAutoSave(options);
					break;
				case Commands.NonSlotComp:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.NonSlotComparison);
					CheckAutoSave(options);
					break;
				case Commands.SlotSummary:
					ShowSlotSummary(options);
					break;
				case Commands.SetSlot:
					options.CurrentFileSaveSlot = GetSaveSlotId(maxSaveSlotId: 4);
					if (options.CurrentFileSaveSlot == default)
						options.ComparisonFileSaveSlot = default;

					CheckAutoSave(options);

					break;
				case Commands.SetCompSlot:
					if (options.CurrentFileSaveSlot != default)
					{
						options.ComparisonFileSaveSlot = GetSaveSlotId(maxSaveSlotId: 4);
						CheckAutoSave(options);
					}
					else
						ConsolePrinter.PrintError(Resources.ErrorCompSaveSlotSetButNotForCurrFile);

					break;
				case Commands.OverwriteComp:
					OverwriteComparisonFileWithCurrentFile(options);
					break;
				case Commands.Backup:
					BackupSaveFile(options, SaveFileKind.CurrentFile, false);
					break;
				case Commands.Restore:
					BackupSaveFile(options, SaveFileKind.CurrentFile, true);
					break;
				case Commands.BackupComp:
					BackupSaveFile(options, SaveFileKind.ComparisonFile, false);
					break;
				case Commands.RestoreComp:
					BackupSaveFile(options, SaveFileKind.ComparisonFile, true);
					break;
				case Commands.Transfer:
					TransferSramToOtherGameFile(options);
					break;
				case Commands.Offset:
					PrintOffsetValue(options);

					break;
				case Commands.EditOffset:
					SaveOffsetValue(options);

					break;
				case Commands.Lang:
					SetUILanguage(options);
					CheckAutoSave(options);

					break;
				case Commands.CompLang:
					SetComparionResultLanguage(options);
					CheckAutoSave(options);

					break;
				case Commands.WatchFile:
					options.AutoWatch = true;
					FileWatcherHelper.StartWatching(options);
					CheckAutoSave(options);

					break;
				case Commands.UnwatchFile:
					options.AutoWatch = false;
					FileWatcherHelper.StopWatching(options);
					CheckAutoSave(options);

					break;
				case Commands.LoadConfig:
					LoadConfig(options, GetConfigName());

					break;
				case Commands.SaveConfig:
					SaveConfig(options, GetConfigName());

					break;
				case Commands.OpenConfig:
					OpenConfig(options, GetConfigName());

					break;
				case Commands.OpenLog:
					OpenLog(options);

					break;
				case Commands.AutoLoadOn:
					options.ConfigPath = $"{GetConfigName() ?? DefaultConfigName}.json";
					SaveConfig(options, DefaultConfigFileName);

					break;
				case Commands.AutoLoadOff:
					options.ConfigPath = null;
					SaveConfig(options, DefaultConfigFileName);

					break;
				case Commands.AutoSaveOn:
					options.AutoSave = true;
					SaveConfig(options, DefaultConfigFileName);

					break;
				case Commands.AutoSaveOff:
					options.AutoSave = false;
					SaveConfig(options, DefaultConfigFileName);

					break;
				case Commands.CreateBindings:
					CreateKeyBindingsFile<Commands>();

					break;
				case Commands.OpenBindings:
					OpenKeyBindingsFile();

					break;
				case Commands.Clear:
					ConsolePrinter.Clear();
					break;
				case Commands.Quit:
					return false;
				default:
					ConsolePrinter.PrintCommands();
					ConsolePrinter.PrintError(Resources.ErrorNoValidCommandCmdTemplate.InsertArgs(command, nameof(Commands.Help)));

					break;
			}

			return true;
		}

		#endregion Command Handling

		#region Compare S-RAM

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(IOptions)"/>
		public virtual int Compare<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			var comparisonFilePath = FilePathHelper.GetComparisonFilePath(options);
			Requires.FileExists(comparisonFilePath, nameof(options.ComparisonPath), Resources.ErrorComparisonFileDoesNotExistTemplate);

			using var currFileStream = (Stream)new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var compFileStream = (Stream)new FileStream(comparisonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			return Compare<TComparer>(currFileStream, compFileStream, options);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(IOptions, TextWriter)"/>
		public int Compare<TComparer>(in IOptions options, in TextWriter output) 
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			try
			{
				using (new TemporaryConsoleOutputSetter(output))
					return Compare<TComparer>(options);
			}
			finally
			{
				ConsolePrinter.ResetColor();
			}
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(Stream, Stream, IOptions, TextWriter)"/>
		public virtual int Compare<TComparer>(Stream currStream, Stream compStream, in IOptions options, in TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			using (new TemporaryConsoleOutputSetter(output))
				return Compare<TComparer>(currStream, compStream, options);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(Stream, Stream, IOptions)"/>
		public virtual int Compare<TComparer>(Stream currStream, Stream compStream, in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			ConvertStreamIfSavestate(options, ref currStream, options.CurrentFilePath!);
			ConvertStreamIfSavestate(options, ref compStream, FilePathHelper.GetComparisonFilePath(options));

			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);
			var compFile = ClassFactory.Create<TSramFile>(compStream, options.GameRegion);
			var comparer = ClassFactory.Create<TComparer>(ConsolePrinter);

			try
			{
				TrySetCulture(options.ComparisonResultLanguage);

				var changedBytes = comparer.CompareSram(currFile, compFile, options);
				if (changedBytes == 0) return 0;

				var export = options.AutoWatch && options.FileWatchFlags.HasUInt32Flag(FileWatchFlags.AutoExport) ||
				             options.ComparisonFlags.HasUInt32Flag(ComparisonFlags.AutoExport);

				if (!export) return changedBytes;

				// Enforce English for exports
				TrySetCulture("en");

				using var ms = new MemoryStream();
				{
					using (var writer = new StreamWriter(ms, leaveOpen: true))
						comparer.CompareSram(currFile, compFile, options, writer);
					
					if (export && options.LogFlags.HasUInt32Flag(LogFlags.Export) || options.LogFlags.HasUInt32Flag(LogFlags.Comparison))
						LogExport(ms, options);

					if (export)
					{
						Export(ms, options);

						var exportFlags = options.ExportFlags;
						var comparisonFilePath = FilePathHelper.GetComparisonFilePath(options);

						if (exportFlags.HasUInt32Flag(ExportFlags.DeleteComp))
						{
							File.Delete(comparisonFilePath);
							ConsolePrinter.PrintColoredLine(ConsoleColor.DarkRed, Resources.StatusCompFileDeleted);
						}
					}

					var overwriteCompFile = options.AutoWatch && options.FileWatchFlags.HasUInt32Flag(FileWatchFlags.OverwriteComp) ||
					                        export && options.ExportFlags.HasUInt32Flag(ExportFlags.OverwriteComp) || 
											options.ComparisonFlags.HasUInt32Flag(ComparisonFlags.OverwriteComp);
					if (overwriteCompFile)
						OverwriteComparisonFileWithCurrentFile(options);
				}

				return changedBytes;
			}
			finally
			{
				RestoreCulture(options.UILanguage);

				ConsolePrinter.ResetColor();
			}
		}

		protected virtual bool ConvertStreamIfSavestate(IOptions options, ref Stream stream, string? filePath)
		{
			if (filePath is null) return false;

			var fileExtension = Path.GetExtension(filePath).ToLower();
			if (fileExtension == SrmFileExtension) return false;

			if (fileExtension == CompFileExtension)
			{
				fileExtension = Path.GetExtension(filePath.Remove(CompFileExtension)!).ToLower()!;
				if (fileExtension == SrmFileExtension) return false;
			}

			stream = GetSramFromSavestate(options, stream).GetOrThrowIfNull("ConvertedStream");

			return true;
		}

		protected abstract Stream GetSramFromSavestate(IOptions options, Stream stream);

		#endregion Compare S-RAM

		#region Export comparison Result

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportCompResult{TComparer}(IOptions)"/>
		public virtual string? ExportCompResult<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new() => InternalExportCompResult<TComparer>(options.Copy());

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportCompResult{TComparer}(IOptions,string)"/>
		public virtual string? ExportCompResult<TComparer>(in IOptions options, in string filePath)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			var optionsCopy = options.Copy();
			optionsCopy.ExportPath = filePath;
			return InternalExportCompResult<TComparer>(optionsCopy);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportCompResult{TComparer}(IOptions,string)"/>
		private string? InternalExportCompResult<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			options.ComparisonFlags = (ComparisonFlags) options.ComparisonFlags | ComparisonFlags.AutoExport;

			if (Compare<TComparer>(options) == 0) return null;

			return options.ExportPath;
		}

		private void Export(Stream ms, IOptions options)
		{
			string? fileName = null;

			if (options.ExportFlags.HasUInt32Flag(ExportFlags.PromptName))
			{
				fileName = GetExportFileName(Path.IsPathRooted(options.ExportPath)
					? null
					: options.ExportPath!);
				if (fileName != string.Empty && Path.GetExtension(fileName) == string.Empty)
					fileName += ".txt";

				// ReSharper disable once VariableHidesOuterVariable
				string GetExportFileName(string? filePath) => InternalGetStringValue(Resources.PromptEnterExportFileName, Resources.StatusExportFileNameSet.InsertArgs(filePath));
			}

			var filePath = FilePathHelper.GetExportFilePath(options, fileName);
			ms.SaveTo(filePath);

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentComparisonExportedTemplate.InsertArgs(filePath));

			var exportFlags = options.ExportFlags;
			if (exportFlags.HasUInt32Flag(ExportFlags.SelectFile))
				SelectFile(filePath);
			else if (exportFlags.HasUInt32Flag(ExportFlags.OpenFile))
				OpenFile(filePath);
		}

		private static void LogExport(Stream ms, IOptions options) => ms.AppendTo(FilePathHelper.GetLogFilePath(options, DefaultLogFileName));

		private static void SelectFile(string filePath)
		{
			if (!File.Exists(filePath)) return;

			//Clean up file path so it can be navigated OK
			filePath = Path.GetFullPath(filePath);
			Process.Start("explorer.exe", $"/select,\"{filePath}\"");
		}

		private static void OpenFile(string filePath)
		{
			if (!File.Exists(filePath)) return;

			//Clean up file path so it can be navigated OK
			filePath = Path.GetFullPath(filePath);
			Process.Start("explorer.exe", $"\"{filePath}\"");
		}

		#endregion Export comparison Result

		#region Export Flags

		private void SetExportFlags(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintFlags(options.ExportFlags);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.PromptEnterFlags);
			ConsolePrinter.SetForegroundColor(ConsoleColor.Cyan);

			var input = Console.ReadLine();
			if (input == string.Empty) return;

			if (!Enum.TryParse(options.ExportFlags.GetType(), input, true, out var result))
				throw new  ArgumentException($"{Resources.ErrorInvalidFlags} {input}");

			options.ExportFlags = (Enum)result!;

			ConsolePrinter.PrintColored(ConsoleColor.Yellow, $" {Resources.StatusExportFlagsSet} ");
			ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, options.ExportFlags.ToFlagsString());
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		#endregion

		#region Get / Set Offset Value

		public virtual void PrintOffsetValue(IOptions options)
		{
			var filePath = options.CurrentFilePath!;
			Requires.FileExists(filePath, nameof(options.CurrentFilePath));

			Stream currStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			ConvertStreamIfSavestate(options, ref currStream, filePath);

			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);

			var offset = GetOffset(out var slotIndex);
			if (offset == 0)
			{
				ConsolePrinter.PrintError(Resources.ErrorOperationAborted);
				return;
			}

			var byteValue = currFile.GetOffsetByte(slotIndex, offset);

			var valueDisplayText = NumberFormatter.GetByteValueRepresentations(byteValue);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusGetOffsetValueTemplate.InsertArgs(offset, valueDisplayText));
			ConsolePrinter.ResetColor();
		}

		public virtual void SaveOffsetValue(IOptions options)
		{
			Requires.FileExists(options.CurrentFilePath, nameof(options.CurrentFilePath));

			var extension = Path.GetExtension(options.CurrentFilePath);
			if (extension != ".srm" && extension != ".comp")
				throw new NotSupportedException(Resources.ErrorSavestateOffsetEditNotSupported);

			var offset = GetOffset(out var slotIndex);
			if (offset == 0)
			{
				ConsolePrinter.PrintError(Resources.ErrorOperationAborted);
				return;
			}

			var value = GetSaveSlotOffsetValue();
			var bytes = value switch
			{
				< 256 => new[] { (byte)value },
				< 256 * 256 => BitConverter.GetBytes((ushort)value),
				_ => BitConverter.GetBytes(value),
			};

			var promptResult = InternalGetStringValue(Resources.PromptCreateNewFileInsteadOfOverwriting);
			var createNewFile = promptResult switch
			{
				"1" => true,
				"2" => false,
				_ => throw new OperationCanceledException(),
			};

			var saveFilePath = options.CurrentFilePath!;
			if (createNewFile)
				saveFilePath += ".manipulated";

			using var currStream = new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.None);
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);

			currFile.SetOffsetBytes(slotIndex, offset, bytes);
			currFile.RawSave(saveFilePath);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusSetOffsetValueTemplate.InsertArgs(value, offset));
			var fileName = Path.GetFileName(saveFilePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, createNewFile
				? Resources.StatusModifiedFileSavedAsTemplate.InsertArgs(fileName)
				: Resources.StatusModifiedFileOverwrittenTemplate.InsertArgs(fileName));
			ConsolePrinter.ResetColor();
		}

		private int GetSaveSlotOffset(int slotIndex) => (int)InternalGetValue(Resources.PromptEnterSaveSlotOffsetTemplate.InsertArgs(slotIndex + 1), Resources.StatusOffsetWillBeUsedTemplate);
		private uint GetSaveSlotOffsetValue() => InternalGetValue(Resources.PromptEnterSaveSlotOffsetValue, Resources.StatusOffsetValueWillBeUsedTemplate);

		private string InternalGetStringValue(string prompt, string? promptResultTemplate = null)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, prompt);
			ConsolePrinter.PrintColoredLine(ConsoleColor.White, "");
			
			var input = Console.ReadLine()!;

			ConsolePrinter.PrintLine();
			if (promptResultTemplate is not null)
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, promptResultTemplate.InsertArgs(input));
				ConsolePrinter.PrintLine();
			}

			ConsolePrinter.ResetColor();

			return input;
		}

		private uint InternalGetValue(string prompt, string promtResultTemplate)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, prompt);
			ConsolePrinter.ResetColor();

			var input = Console.ReadLine()!;

			uint.TryParse(input, out var offset);

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, promtResultTemplate.InsertArgs(offset));

			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
			return offset;
		}

		private int GetOffset(out int slotIndex)
		{
			var promptResult = InternalGetStringValue(Resources.PromptSetSingleSaveSlotTemplate.InsertArgs(4),
				Resources.StatusSetSingleSaveSlotMaxTemplate);
			if (!int.TryParse(promptResult, out var saveSlotId) || saveSlotId > 4)
			{
				ConsolePrinter.PrintError(Resources.ErrorInvalidIndex);
				slotIndex = -1;
				return 0;
			}

			slotIndex = saveSlotId - 1;

			return GetSaveSlotOffset(slotIndex);
		}

		#endregion Get / Set Offset Value

		#region Overwrite Comparison file

		public virtual void OverwriteComparisonFileWithCurrentFile(in IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();

			File.Copy(options.CurrentFilePath!, FilePathHelper.GetComparisonFilePath(options), true);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentFileSaved);
			ConsolePrinter.ResetColor();
		}

		#endregion Overwrite Comparison file

		#region GetSaveSlotId

		private void ShowSlotSummary(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.ResetColor();

			var slotId = GetSaveSlotId();
			if (slotId == 0) return;

			Stream currStream = new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);
			ConvertStreamIfSavestate(options, ref currStream, options.CurrentFilePath!);

			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);
			var summary = currFile.GetSegment(slotId - 1).ToString()!;

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.White, summary);
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		private int GetSaveSlotId() => (int)InternalGetValue("Please set saveslot Id to load (1-4)", "Saveslot {0} will be used");

		public virtual int GetSaveSlotId(in int maxSaveSlotId)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptSetSaveSlotTemplate.InsertArgs(maxSaveSlotId));
			ConsolePrinter.ResetColor();

			var input = Console.ReadLine()!;

			int.TryParse(input, out var saveSlotId);

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, saveSlotId > 0
				? string.Format(Resources.StatusSingleSaveSlotComparisonTemplate, saveSlotId)
				: Resources.StatusAllSaveSlotsComparison);

			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();

			return saveSlotId;
		}

		#endregion GetSaveSlotId

		#region Enum

		public Enum InvertIncludeFlag(in Enum flags, in Enum flag)
		{
			var flagsCopy = flags.InvertUInt32Flags(flag);

			ConsolePrinter.PrintInvertIncludeFlag(flagsCopy, flag);

			return flagsCopy;
		}

		private void SetComparisonFlags(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintFlags(options.ComparisonFlags);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.PromptEnterFlags);
			ConsolePrinter.SetForegroundColor(ConsoleColor.Cyan);

			var input = Console.ReadLine();
			if (input == string.Empty) return;

			if (!Enum.TryParse(options.ComparisonFlags.GetType(), input, true, out var result))
				throw new ArgumentException($"{Resources.ErrorInvalidFlags} [{input}]");

			options.ComparisonFlags = (Enum)result!;

			ConsolePrinter.PrintColored(ConsoleColor.Yellow, $" {Resources.StatusComparisonFlagsSet} ");
			ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, options.ComparisonFlags.ToFlagsString());
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		#endregion Enum

		#region Transfer Save File

		public virtual void TransferSramToOtherGameFile(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			var directoryPath = Path.GetDirectoryName(options.CurrentFilePath)!;
			var extension = Path.GetExtension(options.CurrentFilePath);
			var files = Directory.GetFiles(directoryPath, $"*{extension}").Where(f => f != options.CurrentFilePath).ToArray();
			if (files.Length == 0)
			{
				ConsolePrinter.PrintLine(Resources.StatusNoAvailableOtherFiles);
				return;
			}

			var targeFilepath = GetTargetFilePath();
			if (targeFilepath is null)
				return;

			var targetBackupFilepath = targeFilepath + BackupFileExtension;
			if (!File.Exists(targetBackupFilepath))
			{
				File.Copy(targeFilepath, targetBackupFilepath);
				ConsolePrinter.PrintColored(ConsoleColor.DarkGreen, Resources.StatusFileBackedUpTemplate.InsertArgs(Path.GetFileName(targetBackupFilepath)));
			}

			File.Copy(options.CurrentFilePath!, targeFilepath, true);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrFileSavedAsTemplate.InsertArgs(Path.GetFileName(targeFilepath)));

			string? GetTargetFilePath()
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.EnterIndexOfFileToOverwriteTemplate.InsertArgs(files.Length - 1));
				ConsolePrinter.PrintLine();
				ConsolePrinter.ResetColor();

				var i = 0;
				foreach (var fileName in files)
				{
					ConsolePrinter.PrintColored(ConsoleColor.Cyan, i++.ToString());
					ConsolePrinter.PrintColored(ConsoleColor.White, $@": {Path.GetFileNameWithoutExtension(fileName)}");
				}

				ConsolePrinter.ResetColor();

				var input = Console.ReadLine();

				if (!int.TryParse(input, out var index) || index >= files.Length)
				{
					ConsolePrinter.PrintError(Resources.ErrorInvalidIndex);
					return null;
				}

				return files[index];
			}
		}

		#endregion Transfer Save File

		#region Backup / Restore

		public virtual void BackupSaveFile(in IOptions options, in SaveFileKind fileKind, in bool restore = false)
		{
			var filePath = fileKind == SaveFileKind.CurrentFile ? options.CurrentFilePath! : FilePathHelper.GetComparisonFilePath(options);
			var fileTypeName = fileKind.GetDisplayName();
			var backupFilepath = filePath + BackupFileExtension;

			ConsolePrinter.PrintSectionHeader();

			if (restore)
			{
				File.Copy(backupFilepath, filePath, true);
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusFileRestoredFromBackupTemplate.InsertArgs(fileTypeName));
			}
			else
			{
				File.Copy(filePath, backupFilepath, true);
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrFileBackedUpTemplate.InsertArgs(fileTypeName));
			}

			ConsolePrinter.ResetColor();
		}

		#endregion Backup / Restore

		#region Language

		private void SetUILanguage(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptSetUILanguage);
			ConsolePrinter.ResetColor();

			var cultureId = Console.ReadLine()!;
			if (cultureId == string.Empty)
			{
				options.UILanguage = null;
				RestoreCulture(null);
				ConsolePrinter.PrintConfig(options);
				return;
			}

			CultureInfo culture;

			try
			{
				culture = CultureInfo.GetCultureInfo(cultureId);
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintError(ex);
				return;
			}

			options.UILanguage = culture.Name;
			CultureInfo.CurrentUICulture = culture;

			ConsolePrinter.PrintConfig(options);
		}

		private void SetComparionResultLanguage(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptSetComparionResultLanguage);
			ConsolePrinter.ResetColor();

			var cultureId = Console.ReadLine()!;
			if (cultureId == string.Empty)
			{
				options.ComparisonResultLanguage = null;
				ConsolePrinter.PrintConfig(options);
				return;
			}

			CultureInfo culture;

			try
			{
				culture = CultureInfo.GetCultureInfo(cultureId);
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintError(ex);
				return;
			}

			options.ComparisonResultLanguage = culture.Name;

			ConsolePrinter.PrintConfig(options);
		}

		private void TrySetCulture(string? culture)
		{
			try
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture ?? "en");
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintError(ex.Message);
				ConsolePrinter.PrintSectionHeader();
			}
		}

		private void RestoreCulture(string? culture)
		{
			try
			{
				CultureInfo.CurrentUICulture = culture is null ? CultureInfo.InstalledUICulture : CultureInfo.GetCultureInfo(culture);
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintError(ex.Message);
				ConsolePrinter.PrintSectionHeader();
			}
		}

		#endregion Language

		#region Config

		private void CheckAutoSave(IOptions options)
		{
			if (!options.AutoSave) return;

			SaveConfig(options, DefaultConfigFileName);
		}

		protected virtual void SaveConfig(IOptions options, string? configName = null)
		{
			ConsolePrinter.PrintSectionHeader();

			var filePath = GetConfigFilePath(options.ConfigPath, configName);
			JsonFileSerializer.Serialize(filePath, options);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusConfigFileSavedTemplate.InsertArgs(filePath));
			ConsolePrinter.ResetColor();
		}

		protected virtual void LoadConfig(IOptions options, string? configName = null) => throw new NotImplementedException();

		protected virtual void OpenConfig(IOptions options, string? configName = null)
		{
			var filePath = GetConfigFilePath(options.ConfigPath, configName);
			Requires.FileExists(filePath, string.Empty, Resources.ErrorConfigFileDoesNotExistTemplate.InsertArgs(filePath));

			OpenFile(filePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusConfigFileWillBeOpenedTemplate.InsertArgs(filePath));
			ConsolePrinter.ResetColor();
		}

		protected virtual void OpenLog(IOptions options)
		{
			var filePath = FilePathHelper.GetLogFilePath(options, DefaultLogFileName);

			if (!File.Exists(filePath))
				File.CreateText(filePath).Dispose();
			
			OpenFile(filePath);
			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusLogFileWillBeOpenedTemplate.InsertArgs(filePath));
			ConsolePrinter.ResetColor();
		}

		protected virtual string GetConfigFilePath(string? filePath, string? fileName = null)
		{
			if (fileName is null) return filePath ?? DefaultConfigFileName;

			var extension = Path.GetExtension(fileName);
			return extension switch
			{
				"" => $"{fileName}.json",
				_ => fileName
			};
		}

		private string? GetConfigName()
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptEnterConfigName);
			ConsolePrinter.ResetColor();

			var configName = Console.ReadLine();
			if (configName == string.Empty) return null;

			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();

			return configName;
		}

		#endregion Config

		#region Key Bindings

		protected virtual bool CheckCustomKeyBinding(string command, [NotNullWhen(true)] out string? boundCommand)
		{
			boundCommand = null;

			if (File.Exists(KeyBindingsFileName))
			{
				var keyBindings = JsonFileSerializer.Deserialize<Dictionary<string, string>>(KeyBindingsFileName)!;
				if (keyBindings.SingleOrDefault(e => e.Key.EqualsInsensitive(command)).Value is { } newKey)
				{
					boundCommand = newKey;
					return true;
				}
			}

			return false;
		}

		protected virtual void OpenKeyBindingsFile()
		{
			var keyBindingsPath = Path.Join(Environment.CurrentDirectory, KeyBindingsFileName);
			Requires.FileExists(keyBindingsPath, string.Empty, Resources.ErrorKeyBindingsFileDoesNotExistTemplate.InsertArgs(keyBindingsPath));

			OpenFile(KeyBindingsFileName);
		}

		protected virtual void CreateKeyBindingsFile<TEnum>() where TEnum : struct, Enum
		{
			var bindings = default(TEnum).ToDictionary();
			var keyBindingsPath = Path.Join(Environment.CurrentDirectory, KeyBindingsFileName);
			JsonFileSerializer.Serialize(keyBindingsPath, bindings);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusKeyBindingsFileSavedTemplate.InsertArgs(keyBindingsPath));
			ConsolePrinter.ResetColor();
		}

		#endregion Key Bindings

		#region Logging Flags

		private void SetLogFlags(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintFlags(options.LogFlags);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.PromptEnterFlags);
			ConsolePrinter.SetForegroundColor(ConsoleColor.Cyan);

			var input = Console.ReadLine();
			if (input == string.Empty) return;

			if (!Enum.TryParse(options.LogFlags.GetType(), input, true, out var result))
				throw new ArgumentException($"{Resources.ErrorInvalidFlags} [{input}]");

			options.LogFlags = (Enum)result!;

			ConsolePrinter.PrintColored(ConsoleColor.Yellow, $" {Resources.StatusLogFlagsSet} ");
			ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, options.LogFlags.ToFlagsString());
			ConsolePrinter.ResetColor();
		}

		#endregion

		#region File Watch Flags

		private void SetFileWatchFlags(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintFlags(options.FileWatchFlags);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.PromptEnterFlags);
			ConsolePrinter.SetForegroundColor(ConsoleColor.Cyan);

			var input = Console.ReadLine();
			if (input == string.Empty) return;

			if (!Enum.TryParse(options.FileWatchFlags.GetType(), input, true, out var result))
				throw new ArgumentException($"{Resources.ErrorInvalidFlags} [{input}]");

			options.FileWatchFlags = (Enum)result!;

			ConsolePrinter.PrintColored(ConsoleColor.Yellow, $" {Resources.StatusFileWatchFlagsSet} ");
			ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, options.FileWatchFlags.ToFlagsString());
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		#endregion
	}
}
