using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Shared.Min.Extensions;
using Common.Shared.Min.Helpers;
using SavestateFormat.Snes9x.Extensions;
using SramCommons.Extensions;
using SramCommons.Models;
using SramComparer.Enums;
using SramComparer.Extensions;
using SramComparer.Helpers;
using SramComparer.Properties;
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable StaticMemberInGenericType

namespace SramComparer.Services
{
	/// <summary>
	/// This class handles all standard commands
	/// </summary>
	/// <typeparam name="TSramFile">The SRAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The SRAM game structure</typeparam>
	public class CommandHandler<TSramFile, TSaveSlot> : ICommandHandler<TSramFile, TSaveSlot>
		where TSramFile : SramFile, ISramFile<TSaveSlot>
		where TSaveSlot : struct
	{
		private const string BackupFileExtension = ".backup";
		private const string Snes9x = "snes9x";
		private const string SrmFileExtension = ".srm";
		private const string DefaultConfigName = "Config";

		public static readonly string KeyBindingsFileName = "KeyBindings.json";
		public static readonly string DefaultConfigFileName = $"{DefaultConfigName}.json";

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
		/// <param name="outStream">The optionl stream the output should be written to if not to standard console</param>
		/// <returns>False if the game command loop should exit, otherwise true</returns>
		public virtual bool RunCommand(string command, IOptions options, TextWriter? outStream = null)
		{
			ConsoleHelper.SetInitialConsoleSize();

			var defaultStream = Console.Out;

			if (outStream is not null)
				Console.SetOut(outStream);

			if (options.CurrentFilePath.IsNullOrEmpty())
			{
				ConsolePrinter.PrintFatalError(Resources.ErrorMissingPathArguments);
				Console.ReadKey();
				return true;
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
				case Commands.Help:
					ConsolePrinter.PrintCommands();
					break;
				case Commands.Config:
					ConsolePrinter.PrintConfig(options);
					break;
				case Commands.Guide_Srm:
					ConsolePrinter.PrintGuide("guide-srm");
					break;
				case Commands.Guide_Savestate:
					ConsolePrinter.PrintGuide("guide-savestate");
					break;
				case Commands.Sbc:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.SlotByteByByteComparison);
					break;
				case Commands.Nsbc:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.NonSlotByteByByteComparison);
					break;
				case Commands.SetSlot:
					options.CurrentFileSaveSlot = GetSaveSlotId(maxSaveSlotId: 4);
					if (options.CurrentFileSaveSlot == default)
						options.ComparisonFileSaveSlot = default;

					break;
				case Commands.SetSlot_Comp:
					if (options.CurrentFileSaveSlot != default)
						options.ComparisonFileSaveSlot = GetSaveSlotId(maxSaveSlotId: 4);
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
				case Commands.Backup_Comp:
					BackupSaveFile(options, SaveFileKind.ComparisonFile, false);
					break;
				case Commands.Restore_Comp:
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

					break;
				case Commands.Lang_Comp:
					SetComparionResultLanguage(options);

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
				case Commands.AutoLoadOn:
					options.ConfigFilePath = $"{GetConfigName() ?? DefaultConfigName}.json";
					SaveConfig(options, DefaultConfigFileName);

					break;
				case Commands.AutoLoadOff:
					options.ConfigFilePath = null;
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

		#region Compare SRAM

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(IOptions)"/>
		public virtual void Compare<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			var comparisonFilePath = FileNameHelper.GetComparisonFilePath(options);
			Requires.FileExists(comparisonFilePath, nameof(options.ComparisonFilePath), Resources.ErrorComparisonFileDoesNotExist);
			
			using var currFileStream = (Stream)new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read);
			using var compFileStream = (Stream)new FileStream(comparisonFilePath, FileMode.Open, FileAccess.Read);
			
			Compare<TComparer>(currFileStream, compFileStream, options);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(IOptions, TextWriter)"/>
		public void Compare<TComparer>(IOptions options, TextWriter output) 
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			using (new TemporaryOutputSetter(output))
				Compare<TComparer>(options);

			ConsolePrinter.ResetColor();
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(Stream, Stream, IOptions, TextWriter)"/>
		public virtual void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options, TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			using (new TemporaryOutputSetter(output))
				Compare<TComparer>(currStream, compStream, options);
		}

		private class TemporaryOutputSetter : IDisposable
		{
			private readonly TextWriter _oldOut = Console.Out;

			public TemporaryOutputSetter(TextWriter output) => Console.SetOut(output);

			public void Dispose() => Console.SetOut(_oldOut);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.Compare{TComparer}(Stream, Stream, IOptions)"/>
		public virtual void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			ConvertStreamIfSavestate(ref currStream, options.CurrentFilePath!, options.SavestateType);
			ConvertStreamIfSavestate(ref compStream, FileNameHelper.GetComparisonFilePath(options), options.SavestateType);

			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);
			var compFile = ClassFactory.Create<TSramFile>(compStream, options.GameRegion);
			var comparer = ClassFactory.Create<TComparer>(ConsolePrinter);

			if (options.ComparisonResultLanguage is not null)
				TrySetCulture(options.ComparisonResultLanguage);

			try
			{
				comparer.CompareSram(currFile, compFile, options);
			}
			finally
			{
				if (options.ComparisonResultLanguage is not null)
					RestoreCulture(options.UILanguage);

				ConsolePrinter.ResetColor();
			}
		}

		protected virtual bool ConvertStreamIfSavestate(ref Stream stream, string? filePath, string? savestateType)
		{
			if (filePath is null) return false;

			savestateType ??= Snes9x;

			var fileExtension = Path.GetExtension(filePath).ToLower();
			if (fileExtension == SrmFileExtension) return false;

			var convertedStream = savestateType switch
			{
				Snes9x => stream.ConvertSnes9xSavestateToSram(),
				_ => throw new NotSupportedException($"Savestate type {savestateType} is not supported.")
			};

			stream = convertedStream;

			return true;
		}

		#endregion Compare SRAM

		#region Export comparison Result

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportComparisonResult{TComparer}(IOptions)"/>
		public virtual string ExportComparisonResult<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			var fileName = Path.GetFileNameWithoutExtension(options.CurrentFilePath)!;
			var exportFileName = FileNameHelper.GenerateExportSaveFileName(fileName);
			var filePath = Path.Join(options.ExportDirectory ?? Path.GetDirectoryName(options.CurrentFilePath), exportFileName);

			ExportComparisonResult<TComparer>(options, filePath);

			return filePath;
		}

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.ExportDirectory"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="showInExplorer">Sets if the file should be selected in windows explorer</param>
		/// /// <returns>The generated filepath</returns>
		public virtual string ExportComparisonResult<TComparer>(IOptions options, bool showInExplorer)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			var filePath = ExportComparisonResult<TComparer>(options);

			if (showInExplorer)
				SelectFile(filePath);

			return filePath;
		}

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.ExportDirectory"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="filePath"></param>
		/// <param name="showInExplorer">Sets if the file should be selected in windows explorer</param>
		public virtual void ExportComparisonResult<TComparer>(IOptions options, string filePath, bool showInExplorer)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			ExportComparisonResult<TComparer>(options, filePath);

			if (showInExplorer)
				SelectFile(filePath);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSaveSlot}.ExportComparisonResult{TComparer}(IOptions,string)"/>
		public virtual void ExportComparisonResult<TComparer>(IOptions options, string filePath)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new()
		{
			try
			{
				using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
				using var writer = new StreamWriter(fileStream);

				Compare<TComparer>(options, writer);

				writer.Close();
				fileStream.Close();
				ConsolePrinter.PrintParagraph();
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow,
					Resources.StatusCurrentComparisonExportedTemplate.InsertArgs(filePath));
			}
			catch (Exception ex)
			{
				throw new Exception(Resources.ErrorCannotOpenOutputFileTemplate.InsertArgs(filePath) +
				                    Environment.NewLine + ex.Message);
			}

			ConsolePrinter.ResetColor();
		}

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
				ConsolePrinter.PrintColored(ConsoleColor.DarkGreen ,Resources.StatusFileBackedUpTemplate.InsertArgs(Path.GetFileName(targetBackupFilepath)));
			}

			File.Copy(options.CurrentFilePath!, targeFilepath, true);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrFileSavedAsTemplate.InsertArgs(Path.GetFileName(targeFilepath)));

			string? GetTargetFilePath()
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.EnterIndexOfFileToOverwriteTemplate.InsertArgs(files.Length - 1));
				ConsolePrinter.PrintParagraph();
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

		#region InvertIncludeFlag

		public Enum InvertIncludeFlag(in Enum flags, in Enum flag)
		{
			var enumType = flags.GetType();
			var enumFlag = (Enum)Enum.ToObject(enumType, flag);

			var flagsCopy = EnumHelper.InvertUIntFlag(flags, enumFlag);

			ConsolePrinter.PrintInvertIncludeFlag(flagsCopy, enumFlag);

			return flagsCopy;
		}

		#endregion InvertIncludeFlag

		#region Get / Set Offset Value

		public virtual void PrintOffsetValue(IOptions options)
		{
			var filePath = options.CurrentFilePath!;
			Requires.FileExists(filePath, nameof(options.CurrentFilePath));

			Stream currStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

			ConvertStreamIfSavestate(ref currStream, filePath, options.SavestateType);

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

			using var currStream = new FileStream(options.CurrentFilePath!, FileMode.Open, FileAccess.Read);
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);

			currFile.SetOffsetBytes(slotIndex, offset, bytes);
			currFile.RawSave(saveFilePath);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusSetOffsetValueTemplate.InsertArgs(value, offset));
			var fileName = Path.GetFileName(saveFilePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, createNewFile
				? Resources.StatusModifiedFileHasBeenSavedAsTemplate.InsertArgs(fileName)
				: Resources.StatusModifiedFileHasBeenOverwrittenTemplate.InsertArgs(fileName));
			ConsolePrinter.ResetColor();
		}

		private int GetSaveSlotOffset(int slotIndex) => (int)InternalGetValue(Resources.PromptGetSaveSlotOffsetTemplate.InsertArgs(slotIndex + 1), Resources.StatusOffsetWillBeUsedTemplate);
		private uint GetSaveSlotOffsetValue() => InternalGetValue(Resources.PromptGetSaveSlotOffsetValue, Resources.StatusOffsetValueWillBeUsedTemplate);

		private string InternalGetStringValue(string prompt, string? promptResultTemplate = null)
		{
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, prompt);
			ConsolePrinter.ResetColor();

			var input = Console.ReadLine()!;

			ConsolePrinter.PrintParagraph();
			if (promptResultTemplate is not null)
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, promptResultTemplate.InsertArgs(input));
				ConsolePrinter.PrintParagraph();
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

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.PrintColoredLine(ConsoleColor.DarkYellow, promtResultTemplate.InsertArgs(offset));

			ConsolePrinter.PrintParagraph();
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

		#region GetSaveSlotId

		public virtual int GetSaveSlotId(int maxSaveSlotId)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptSetSaveSlotTemplate.InsertArgs(maxSaveSlotId));
			ConsolePrinter.ResetColor();

			var input = Console.ReadLine()!;

			int.TryParse(input, out var saveSlotId);

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, saveSlotId > 0
				? string.Format(Resources.StatusSingleSaveSlotComparisonTemplate, saveSlotId)
				: Resources.StatusAllSaveSlotsWillBeCompared);

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.ResetColor();

			return saveSlotId;
		}

		#endregion GetSaveSlotId

		#region Overwrite Comparison file

		public virtual void OverwriteComparisonFileWithCurrentFile(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();

			File.Copy(options.CurrentFilePath!, FileNameHelper.GetComparisonFilePath(options), true);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentFileHasBeenSaved);
			ConsolePrinter.ResetColor();
		}

		#endregion Overwrite Comparison file

		#region Backup / Restore

		public virtual void BackupSaveFile(IOptions options, SaveFileKind fileKind, bool restore = false)
		{
			var filePath = fileKind == SaveFileKind.CurrentFile ? options.CurrentFilePath! : FileNameHelper.GetComparisonFilePath(options);
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

		private void TrySetCulture(string culture)
		{
			try
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
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

		protected virtual void SaveConfig(IOptions options, string? configName = null)
		{
			ConsolePrinter.PrintSectionHeader();
			var jsonOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }, WriteIndented = true };

			var filePath = GetConfigFilePath(options.ConfigFilePath, configName);
			JsonFileSerializer.Serialize(filePath, options, jsonOptions);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusConfigFileHasBeenSavedTemplate.InsertArgs(filePath));
			ConsolePrinter.ResetColor();
		}

		protected virtual void LoadConfig(IOptions options, string? configName = null) => throw new NotImplementedException();

		protected virtual void OpenConfig(IOptions options, string? configName = null)
		{
			var filePath = GetConfigFilePath(options.ConfigFilePath, configName);
			Requires.FileExists(filePath, string.Empty, Resources.ErrorConfigFileDoesNotExist.InsertArgs(filePath));

			OpenFile(filePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusConfigFileWillBeOpenedTemplate.InsertArgs(filePath));
			ConsolePrinter.ResetColor();
		}

		protected virtual string GetConfigFilePath(string? configFilePath, string? configName = null)
		{
			if (configName is null) return configFilePath ?? DefaultConfigFileName;

			var extension = Path.GetExtension(configName);
			return extension switch
			{
				"" => $"{configName}.json",
				_ => configName
			};
		}

		private string? GetConfigName()
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.PromptEnterConfigName);
			ConsolePrinter.ResetColor();

			var configName = Console.ReadLine();
			if (configName == string.Empty) return null;

			ConsolePrinter.PrintParagraph();
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
			Requires.FileExists(keyBindingsPath, string.Empty, Resources.ErrorKeyBindingsFileDoesNotExist.InsertArgs(keyBindingsPath));

			OpenFile(KeyBindingsFileName);
		}

		protected virtual void CreateKeyBindingsFile<TEnum>() where TEnum: struct, Enum
		{
			var bindings = default(TEnum).ToDictionary();
			var options = new JsonSerializerOptions
				{
					Converters = { new JsonStringEnumObjectConverter() }, 
					WriteIndented = true
				};

			var keyBindingsPath = Path.Join(Environment.CurrentDirectory, KeyBindingsFileName);
			JsonFileSerializer.Serialize(keyBindingsPath, bindings, options);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusKeyBindingsFileSavedTemplate.InsertArgs(keyBindingsPath));
			ConsolePrinter.ResetColor();
		}

		#endregion Key Bindings
	}
}
