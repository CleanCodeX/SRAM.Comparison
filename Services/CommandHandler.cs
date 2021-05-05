using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using Common.Shared.Min.Extensions;
using Common.Shared.Min.Helpers;
using IO.Extensions;
using IO.Helpers;
using IO.Models;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable StaticMemberInGenericType

namespace SRAM.Comparison.Services
{
	public abstract class CommandHandler
	{
		protected static string DefaultConfigName = "Config";
		protected static string DefaultLogFileName = "Log.txt";
		protected const string DefaultUrisFileName = "Uris.json";
		protected internal const string DefaultUpdateFileName = "Update.json";
		protected const string DefaultDownloadDirectory = "Download";
		protected const string DefaultUpdateDirectory = DefaultDownloadDirectory + "/Update";

		public static readonly string KeyBindingsFileName = "KeyBindings.json";
		public static readonly string DefaultConfigFileName = $"{DefaultConfigName}.json";

		public virtual string? AppVersion { get; set; }
	}

	/// <summary>
	/// This class handles all standard commands
	/// </summary>
	/// <typeparam name="TSramFile">The S-RAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The S-RAM game structure</typeparam>
	public abstract class CommandHandler<TSramFile, TSaveSlot> : CommandHandler, ICommandHandler<TSramFile, TSaveSlot>, IAutoUpdater, ISavestateConverter
		where TSramFile : class, IMultiSegmentFile<TSaveSlot>, IRawSave, IStructFile
		where TSaveSlot : struct
	{
		private const string BackupFileExtension = ".backup";

		private const string GuideSrmFileName = "guide-srm";
		private const string GuideSavestateFileName = "guide-savestate";

		protected IConsolePrinter ConsolePrinter { get; }
		
		#region Ctors

		public CommandHandler() : this(ComparisonServices.ConsolePrinter) {}
		/// <param name="consolePrinter">A specific console printer instance</param>
		public CommandHandler(IConsolePrinter consolePrinter) => ConsolePrinter = consolePrinter;

		#endregion Ctors

		protected class Uris
		{
			public string? Downloads { get; set; }
			public string? Docu { get; set; }
			public string? LatestUpdate { get; set; }
			public string? Project { get; set; }
			public string? Forum { get; set; }
			public string? DiscordInvite { get; set; }
		}

		protected class Update
		{
			public DateTime? LastCheckDate { get; set; }
			public bool Download { get; set; }
			public bool Replace { get; set; }
		}

		protected class LatestUpdateInfo
		{
			public DateTimeOffset VersionDate { get; set; }
			public string? Version { get; set; }
			public string? DownloadUri { get; set; }
		}

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
				case Commands.ExportCompResult:
				case Commands.ExportCompResultOpen:
				case Commands.ExportCompResultSelect:
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
				case Commands.ShowSlotSummary:
					ShowSaveSlotSummary(options);
					break;
				case Commands.ExportSlotSummary:
					ExportSaveSlotSummary(options);
					break;
				case Commands.SetSlot:
					options.CurrentFileSaveSlot = GetSaveSlotIdWithStatus(maxSaveSlotId: 4);
					if (options.CurrentFileSaveSlot == default)
						options.ComparisonFileSaveSlot = default;

					CheckAutoSave(options);

					break;
				case Commands.SetCompSlot:
					if (options.CurrentFileSaveSlot != default)
					{
						options.ComparisonFileSaveSlot = GetSaveSlotIdWithStatus(maxSaveSlotId: 4);
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
				case Commands.OpenProject:
				case Commands.OpenDocu:
				case Commands.OpenDownloads:
				case Commands.OpenForum:
				case Commands.OpenDiscordInvite:
					OpenLink(cmd.ToString().Remove("Open")!);
					break;
				case Commands.CheckForUpdate:
					CheckUpdates(false);
					break;
				case Commands.Update:
					CheckUpdates(true);
					break;
				case Commands.EnableDailyUpdateCheck:
				case Commands.DisableDailyUpdateCheck:
					EnableDailyUpdateCheck(cmd == Commands.EnableDailyUpdateCheck);
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
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new() => Compare<TComparer>(options, null);

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(IOptions, TextWriter)"/>
		public int Compare<TComparer>(in IOptions options, in TextWriter? output) 
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			try
			{
				var comparisonFilePath = FilePathHelper.GetComparisonFilePath(options);
				Requires.FileExists(comparisonFilePath, nameof(options.ComparisonPath), Resources.ErrorComparisonFileDoesNotExistTemplate);

				using var currFileStream = (Stream)new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var compFileStream = (Stream)new FileStream(comparisonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

				return Compare<TComparer>(currFileStream, compFileStream, options, output);
			}
			finally
			{
				ConsolePrinter.ResetColor();
			}
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(Stream, Stream, IOptions)"/>
		public virtual int Compare<TComparer>(Stream currStream, Stream compStream, in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new() =>
			Compare<TComparer>(currStream, compStream, options, null);

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(Stream, Stream, IOptions, TextWriter)"/>
		public virtual int Compare<TComparer>(Stream currStream, Stream compStream, in IOptions options, in TextWriter? output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			if(ConvertStreamIfSavestate(options, currStream, options.CurrentFilePath!) is {} convertedCurrData)
				currStream = convertedCurrData.ToStream();
			
			if(ConvertStreamIfSavestate(options, compStream, FilePathHelper.GetComparisonFilePath(options)) is {} convertedCompData)
				compStream = convertedCompData.ToStream();

			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);
			var compFile = ClassFactory.Create<TSramFile>(compStream, options.GameRegion);
			var comparer = ClassFactory.Create<TComparer>(ConsolePrinter);

			try
			{
				TrySetCulture(options.ComparisonResultLanguage);

				var changedBytes = comparer.CompareSram(currFile, compFile, options, output);
				if (changedBytes == 0) return 0;

				var export = options.AutoWatch && options.FileWatchFlags.HasUInt32Flag(FileWatchFlags.AutoExport) ||
				             options.ComparisonFlags.HasUInt32Flag(ComparisonFlags.AutoExport);

				if (!export) return changedBytes;

				// Enforce English for exports
				TrySetCulture("en");

				StringBuilder sb = new();

				comparer.CompareSram(currFile, compFile, options, new StringWriter(sb));

				var compResult = sb.ToString();

				if (export && options.LogFlags.HasUInt32Flag(LogFlags.Export) || options.LogFlags.HasUInt32Flag(LogFlags.Comparison))
					AppendToFile(compResult, options);

				if (export)
				{
					SaveToPromptFile(compResult, options);

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

				return changedBytes;
			}
			finally
			{
				RestoreCulture(options.UILanguage);

				ConsolePrinter.ResetColor();
			}
		}

		public virtual byte[]? ConvertStreamIfSavestate(IOptions options, in Stream stream, string? filePath)
		{
			stream.ThrowIfNull(nameof(stream));

			if (filePath is null) return null;
			if (!FilePathHelper.IsSavestateFile(filePath)) return null;

			return LoadSramFromSavestate(options, stream).GetOrThrowIfNull("ConvertedStream");
		}

		public virtual byte[]? ConvertStreamToByteArrayIfSavestate(IOptions options, in Stream stream, string? filePath)
		{
			stream.ThrowIfNull(nameof(stream));

			if (filePath is null) return null;
			if (!FilePathHelper.IsSavestateFile(filePath)) return null;
			
			FileStream savestateStream = new(filePath, FileMode.Open, FileAccess.Read);
			return SaveSramToSavestate(options, savestateStream, stream);
		}

		protected abstract byte[] LoadSramFromSavestate(IOptions options, in Stream stream);
		protected abstract byte[] SaveSramToSavestate(IOptions options, in Stream savestateStream, in Stream srmStream);

		#endregion Compare S-RAM

		#region Export comparison Result

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportCompResult{TComparer}(IOptions)"/>
		public virtual string? SaveCompResult<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new() => InternalSaveCompResult<TComparer>(options.Copy());

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportCompResult{TComparer}(IOptions,string)"/>
		public virtual string? SaveCompResult<TComparer>(in IOptions options, in string filePath)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			var optionsCopy = options.Copy();
			optionsCopy.ExportPath = filePath;
			return InternalSaveCompResult<TComparer>(optionsCopy);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportCompResult{TComparer}(IOptions,string)"/>
		private string? InternalSaveCompResult<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			options.ComparisonFlags = (ComparisonFlags) options.ComparisonFlags | ComparisonFlags.AutoExport;

			return Compare<TComparer>(options, new StringWriter()) > 0
				? options.ExportPath
				: null;
		}

		private void SaveToPromptFile(string contents, IOptions options)
		{
			string? fileName = null;

			if (options.ExportFlags.HasUInt32Flag(ExportFlags.PromptName))
			{
				fileName = GetExportFileName(Path.IsPathRooted(options.ExportPath)
					? null
					: options.ExportPath!);
				if (fileName.IsNotNullOrEmpty() && Path.GetExtension(fileName) == string.Empty)
					fileName += ".txt";

				// ReSharper disable once VariableHidesOuterVariable
				string? GetExportFileName(string? filePath) => InternalGetStringValue(Resources.PromptEnterExportFileName, filePath, Resources.StatusExportFileNameSet);
			}

			var filePath = FilePathHelper.GetExportFilePath(options, fileName);
			File.WriteAllText(filePath, contents);

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentComparisonExportedTemplate.InsertArgs(filePath));

			var exportFlags = options.ExportFlags;
			if (exportFlags.HasUInt32Flag(ExportFlags.SelectFile))
				SelectFile(filePath);
			else if (exportFlags.HasUInt32Flag(ExportFlags.OpenFile))
				OpenFile(filePath);
		}

		private static void AppendToFile(string contents, IOptions options) => File.AppendAllText(FilePathHelper.GetLogFilePath(options, DefaultLogFileName), contents);

		private static void SelectFile(string filePath)
		{
			if (!File.Exists(filePath)) return;

			//Clean up file path so it can be navigated
			filePath = Path.GetFullPath(filePath);
			Process.Start("explorer.exe", $"/select,\"{filePath}\"");
		}

		private static void OpenFile(string filePath)
		{
			if (!File.Exists(filePath)) return;

			//Clean up file path so it can be navigated
			filePath = Path.GetFullPath(filePath);
			Process.Start("explorer.exe", $"\"{filePath}\"");
		}

#endregion Export comparison Result

		public string GetSummary(Stream stream, IOptions options) => GetSaveSlotSummary(stream, options, options.CurrentFileSaveSlot);

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

			Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			if (ConvertStreamIfSavestate(options, stream, options.CurrentFilePath!) is { } convertedData)
				stream = convertedData.ToStream();

			var currFile = ClassFactory.Create<TSramFile>(stream, options.GameRegion);

			var offset = GetOffset(out var slotIndex);
			if (offset == 0)
			{
				ConsolePrinter.PrintError(Resources.ErrorOperationAborted);
				return;
			}

			var byteValue = currFile.GetOffsetByte(slotIndex, offset);

			var valueDisplayText = NumberFormatter.FormatDecHexBin(byteValue);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusGetOffsetValueTemplate.InsertArgs(offset, valueDisplayText));
			ConsolePrinter.ResetColor();
		}

		public virtual void SaveOffsetValue(IOptions options)
		{
			Requires.FileExists(options.CurrentFilePath, nameof(options.CurrentFilePath));

			var offset = GetOffset(out var slotIndex);
			if (offset == 0)
			{
				ConsolePrinter.PrintError(Resources.ErrorOperationAborted);
				return;
			}

			var value = GetSaveSlotOffsetValue();
			var bytes = value switch
			{
				< byte.MaxValue => new[] { (byte)value },
				< ushort.MaxValue => BitConverter.GetBytes((ushort)value),
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

			var filePath = options.CurrentFilePath!;
			Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

			if (ConvertStreamIfSavestate(options, stream, filePath) is { } convertedData)
				stream = convertedData.ToStream();

			var sramFile = ClassFactory.Create<TSramFile>(stream, options.GameRegion);

			sramFile.SetOffsetBytes(slotIndex, offset, bytes);

			using MemoryStream outputStream = new();
			sramFile.Save(outputStream);

			if (ConvertStreamToByteArrayIfSavestate(options, outputStream, filePath) is { } savestateData)
				File.WriteAllBytes(saveFilePath, savestateData);
			else
				File.WriteAllBytes(saveFilePath, outputStream.GetBuffer());

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusSetOffsetValueTemplate.InsertArgs(value, offset));
			var fileName = Path.GetFileName(saveFilePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, createNewFile
				? Resources.StatusModifiedFileSavedAsTemplate.InsertArgs(fileName)
				: Resources.StatusModifiedFileOverwrittenTemplate.InsertArgs(fileName));
			ConsolePrinter.ResetColor();
		}

		private int GetSaveSlotOffset(int slotIndex) => (int)InternalGetValue(Resources.PromptEnterSaveSlotOffsetTemplate.InsertArgs(slotIndex + 1), Resources.StatusOffsetWillBeUsedTemplate);
		private uint GetSaveSlotOffsetValue() => InternalGetValue(Resources.PromptEnterSaveSlotOffsetValue, Resources.StatusOffsetValueWillBeUsedTemplate);

		private string? InternalGetStringValue(string prompt, string? defaultValue = null, string? promptResultTemplate = null)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, prompt);

			if (defaultValue is not null)
				ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, $"{Resources.DefaultValue}: {defaultValue}");
			
			ConsolePrinter.PrintColoredLine(ConsoleColor.White, "");
			
			var input = Console.ReadLine().ToNullIfEmpty();
			var result = input ?? defaultValue;

			if (promptResultTemplate is not null && result is not null)
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, promptResultTemplate.InsertArgs(result));
				ConsolePrinter.PrintLine();
			}

			ConsolePrinter.ResetColor();

			return result;
		}

		private uint InternalGetValue(string prompt, string? promtResultTemplate = null)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, prompt);
			ConsolePrinter.ResetColor();

			var input = Console.ReadLine()!;

			uint.TryParse(input, out var offset);

			if (promtResultTemplate is not null) 
			{
				ConsolePrinter.PrintLine();
				ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, promtResultTemplate.InsertArgs(offset));
			}

			ConsolePrinter.ResetColor();
			return offset;
		}

		private int GetOffset(out int slotIndex)
		{
			var promptResult = InternalGetStringValue(Resources.PromptSetSingleSaveSlotTemplate.InsertArgs(4),
				"1", promptResultTemplate: Resources.StatusSetSingleSaveSlotMaxTemplate);
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

		protected abstract int GetMaxSaveSlotId();

		private void ExportSaveSlotSummary(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.ResetColor();

			var slotId = GetSaveSlotIdNoStatus(GetMaxSaveSlotId());
			if (slotId == 0)
				throw new ArgumentException(Resources.ErrorOperationAborted);

			ConsolePrinter.PrintLine(Resources.StatusSaveSlotToExportTemplate.InsertArgs(slotId));

			var defaultFileName = $"SaveSlot_{slotId}.txt";
			var fileName = GetExportFileName(Path.IsPathRooted(options.ExportPath)
				? defaultFileName
				: Path.Join(options.ExportPath!, defaultFileName));
			if (fileName.IsNotNullOrEmpty() && Path.GetExtension(fileName) == string.Empty)
				fileName += ".txt";

			// ReSharper disable once VariableHidesOuterVariable
			string? GetExportFileName(string? filePath) => InternalGetStringValue(Resources.PromptEnterExportFileName, filePath, Resources.StatusExportFileNameSet.InsertArgs(filePath));

			var filePath = FilePathHelper.GetExportFilePath(options, fileName);

			Stream currStream = new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);

			if (ConvertStreamIfSavestate(options, currStream, options.CurrentFilePath!) is { } convertedData)
				currStream = convertedData.ToStream();

			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);
			var summary = currFile.GetSegment(slotId - 1).ToString()!;

			File.WriteAllText(filePath, summary);

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusSaveSlotSummaryExportedTemplate.InsertArgs(slotId, filePath));

			var exportFlags = options.ExportFlags;
			if (exportFlags.HasUInt32Flag(ExportFlags.SelectFile))
				SelectFile(filePath);
			else if (exportFlags.HasUInt32Flag(ExportFlags.OpenFile))
				OpenFile(filePath);
		}

		private void ShowSaveSlotSummary(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.ResetColor();

			var slotId = GetSaveSlotIdNoStatus(GetMaxSaveSlotId());
			if (slotId == 0)
				throw new ArgumentException(Resources.ErrorOperationAborted);

			ConsolePrinter.PrintLine(Resources.StatusSaveSlotToShowTemplate.InsertArgs(slotId));

			Stream currStream = new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);
			var summary = GetSaveSlotSummary(currStream, options, slotId);

			ConsolePrinter.PrintLine();
			ConsolePrinter.PrintColoredLine(ConsoleColor.White, summary);
			ConsolePrinter.PrintLine();
			ConsolePrinter.ResetColor();
		}

		private string GetSaveSlotSummary(Stream stream, IOptions options, int saveSlotId)
		{
			if (ConvertStreamIfSavestate(options, stream, options.CurrentFilePath!) is { } convertedData)
				stream = convertedData.ToStream();

			var currFile = ClassFactory.Create<TSramFile>(stream, options.GameRegion);
			return currFile.GetSegment(saveSlotId - 1).ToString()!;
		}

		private int GetSaveSlotIdWithStatus(in int maxSaveSlotId) => (int)InternalGetValue(Resources.PromptEnterSaveSlotTemplate.InsertArgs(maxSaveSlotId), Resources.StatusSaveSlotToCompareTemplate);
		private int GetSaveSlotIdNoStatus(in int maxSaveSlotId) => (int) InternalGetValue(Resources.PromptEnterSaveSlotTemplate.InsertArgs(maxSaveSlotId));

		public virtual int GetSaveSlotIdOrAll(in int maxSaveSlotId)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptEnterSaveSlotOrAllTemplate.InsertArgs(maxSaveSlotId));
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

		#region Open websites

		protected virtual void OpenLink(string propertyName)
		{
			const string uriFile = DefaultUrisFileName;
			Uris? uris = null;
			if (File.Exists(uriFile) && JsonFileSerializer.Deserialize<Uris>(uriFile) is { } loadedUris)
				uris = loadedUris;
			else if (GetUris() is { } loadedUris2)
				uris = loadedUris2;

			if (uris is null)
				throw new InvalidOperationException(Resources.ErrorUrlNotDefinedTemplate.InsertArgs("Uris"));

			var uri = (string?)uris.GetType().GetProperty(propertyName)!.GetValue(uris);
			if (uri is null)
				throw new InvalidOperationException(Resources.ErrorUrlNotDefinedTemplate.InsertArgs(propertyName));

			try
			{
				OpenUrl(uri);
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintError(ex.Message + Environment.NewLine + $"URL: {uri}");
			}
		}

		protected virtual Uris? GetUris() => null;

		private static void OpenUrl(string url)
		{
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process.Start("xdg-open", url);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				Process.Start("open", url);
			}
		}

		#endregion

		#region Update

		private void EnableDailyUpdateCheck(bool enable)
		{
			if(enable)
				SaveUpdateConfig(true, true);
			else if(File.Exists(DefaultUpdateFileName))
				File.Delete(DefaultUpdateFileName);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusEnableDailyUpdateCheckTemplate.InsertArgs(Convert.ToByte(enable)));
		}

		private static void SaveUpdateConfig(bool download, bool replace, DateTime? lastUpdateCheck = null) => JsonFileSerializer.Serialize(DefaultUpdateFileName, new Update {Download = download, Replace = replace, LastCheckDate = lastUpdateCheck });

		public virtual void CheckForUpdates()
		{
			var options = JsonFileSerializer.Deserialize<Update>(DefaultUpdateFileName);
			if (options.LastCheckDate >= DateTime.Today)
				return;

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCheckingForUpdates);
			CheckUpdates(options.Download, options.Replace);
		}

		protected virtual void CheckUpdates(bool noPrompts) => CheckUpdates(noPrompts, noPrompts);
		protected virtual void CheckUpdates(bool download, bool replace)
		{
			var uriFile = DefaultUrisFileName;
			string? uri = null;
			if (File.Exists(uriFile) && JsonFileSerializer.Deserialize<Uris>(uriFile) is { LatestUpdate: not null and not "" } uris)
				uri = uris.LatestUpdate!;
			else if (GetUris()?.LatestUpdate is { } latestUpdate and not "")
				uri = latestUpdate;

			if (uri is null)
				throw new InvalidOperationException(Resources.ErrorUrlNotDefinedTemplate.InsertArgs(nameof(Uris.LatestUpdate)));

			try
			{
				using WebClient client = new();
				var latestUpdateJson = client.DownloadString(uri).GetOrThrowIfNull(nameof(Uris.LatestUpdate) + "URL");
				var latestUpdate = JsonSerializer.Deserialize<LatestUpdateInfo>(latestUpdateJson)!;

				latestUpdate.Version.ThrowIfNull(nameof(LatestUpdateInfo.Version));
				latestUpdate.DownloadUri.ThrowIfNull(nameof(LatestUpdateInfo.DownloadUri));

				if (latestUpdate.Version.CompareTo(AppVersion!) <= 0)
				{
					ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusNoUpdateAvailable);
					SaveUpdateConfig(download, replace, DateTime.Today);
					return;
				}

				var versionDate = latestUpdate.VersionDate.ToLocalTime().DateTime.ToShortDateString();
				ConsolePrinter.PrintColoredLine(ConsoleColor.Cyan, Resources.StatusUpdateIsAvailableTemplate.InsertArgs("v" + AppVersion, "v" + latestUpdate.Version, versionDate));

				if (!download)
				{
					ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.PromptDownloadUpdateNow);
					if (Console.ReadLine()!.ToLowerInvariant() != "y")
						throw new OperationCanceledException(Resources.ErrorOperationAborted);
				}

				var downloadDir = DefaultDownloadDirectory;
				DirectoryHelper.EnsureDirectoryExists(downloadDir);

				var downloadUri = latestUpdate.DownloadUri;
				if (!Environment.Is64BitProcess)
					downloadUri = downloadUri.Replace("x64", "x86");

				var fileName = Path.GetFileName(downloadUri);
				var saveFilePath = Path.Join(downloadDir, fileName);

				if (File.Exists(saveFilePath))
					File.Delete(saveFilePath);
				client.DownloadFile(latestUpdate.DownloadUri, saveFilePath);

				var updateDir = DefaultUpdateDirectory;
				DirectoryHelper.EnsureDirectoryNotExists(updateDir);

				ZipFile.ExtractToDirectory(saveFilePath, updateDir, true);
				File.Delete(saveFilePath);

				if (!replace)
				{
					ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusDownloadFinished);
					ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.PromptApplyUpdateNow);
					if (Console.ReadLine()!.ToLowerInvariant() != "y")
						return;
				}

				var firstDirectory = Directory.GetDirectories(updateDir).FirstOrDefault();
				firstDirectory ??= updateDir;

				var appFilePath = "SramComparer.exe";
				MoveDirectoryContents(new(firstDirectory), new(Environment.CurrentDirectory), appFilePath);

				string oldFileName = appFilePath + ".Old";

				File.Move(appFilePath, oldFileName, true);
				File.Move(Path.Join(firstDirectory, appFilePath), appFilePath);
				Directory.Delete(updateDir, true);

				var batFile = "Replace.bat";
				var cmdText = $"del {oldFileName} /f /q\n{appFilePath}";

				File.WriteAllText(batFile, cmdText);

				SaveUpdateConfig(download, replace, DateTime.Today);

				for (var i = 3; i >= 0; --i)
				{
					Console.CursorLeft = 0;
					ConsolePrinter.PrintColored(ConsoleColor.Yellow, Resources.StatusClosingAndReplacingAppTemplate.InsertArgs(i));
					Thread.Sleep(995);
				}

				var cmdParams = $@"/start /b cmd /c ""{batFile}""";
				Process.Start(new ProcessStartInfo("cmd", cmdParams) { CreateNoWindow = false });
				
				Environment.Exit(0);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				ConsolePrinter.PrintError(ex.Message + Environment.NewLine + $"URL: {uri}");
			}
		}

		private static void MoveDirectoryContents(DirectoryInfo source, DirectoryInfo destination, string? exceptFile = null)
		{
			if (!destination.Exists)
				destination.Create();

			// Copy all files.
			var files = source.GetFiles();
			foreach (var file in files)
				if (file.Name != exceptFile)
					file.MoveTo(Path.Join(destination.FullName, file.Name), true);

			// Process subdirectories.
			var dirs = source.GetDirectories();
			foreach (var dir in dirs)
			{
				MoveDirectoryContents(dir, new(Path.Join(destination.FullName, dir.Name)), exceptFile);
				Directory.Delete(dir.FullName);
			}
		}

		#endregion
	}
}
