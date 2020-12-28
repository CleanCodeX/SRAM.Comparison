using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using Common.Shared.Min.Helpers;
using SramCommons.Extensions;
using SramCommons.Models;
using SramComparer.Enums;
using SramComparer.Helpers;
using SramComparer.Properties;
// ReSharper disable RedundantArgumentDefaultValue

namespace SramComparer.Services
{
	/// <summary>
	/// This class handles all standard commands
	/// </summary>
	/// <typeparam name="TSramFile">The SRAM file structure</typeparam>
	/// <typeparam name="TSramGame">The SRAM game structure</typeparam>
	public class CommandHandler<TSramFile, TSramGame> : ICommandHandler<TSramFile, TSramGame>
		where TSramFile : SramFile, ISramFile<TSramGame>
		where TSramGame : struct
	{
		private const string BackupFileExtension = ".backup";

		private IConsolePrinter ConsolePrinter { get; }

		public CommandHandler() : this(ServiceCollection.ConsolePrinter) {}
		/// <param name="consolePrinter">A specific console printer instance</param>
		public CommandHandler(IConsolePrinter consolePrinter) => ConsolePrinter = consolePrinter;

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

			if (options.CurrentSramFilepath.IsNullOrEmpty())
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
			if (command == "?") command = nameof(Commands.cmd);
			
			if (Enum.TryParse<AlternateCommands>(command, true, out var altCommand))
				command = ((Commands)altCommand).ToString();

			var cmd = command.ParseEnum<Commands>();

			switch (cmd)
			{
				case Commands.c:
				case Commands.e:
					throw new NotImplementedException(Resources.ErrorCommandNotImplementedTemplate.InsertArgs(command));
				case Commands.cmd:
					ConsolePrinter.PrintCommands();
					break;
				case Commands.s:
					ConsolePrinter.PrintSettings(options);
					break;
				case Commands.m:
					ConsolePrinter.PrintManual();
					break;
				case Commands.asbc:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.SlotAllBytesComparison);
					break;
				case Commands.nsbc:
					options.ComparisonFlags = InvertIncludeFlag(options.ComparisonFlags, ComparisonFlags.NonSlotBytesComparison);
					break;
				case Commands.ss:
					options.CurrentSramFileSaveSlot = GetGameId(maxGameId: 4);
					if (options.CurrentSramFileSaveSlot == default)
						options.ComparisonSramFileSaveSlot = default;

					break;
				case Commands.ssc:
					if (options.CurrentSramFileSaveSlot != default)
						options.ComparisonSramFileSaveSlot = GetGameId(maxGameId: 4);
					else
						ConsolePrinter.PrintError(Resources.ErrorComparisoGameSetButNotGame);

					break;
				case Commands.ow:
					OverwriteComparisonFileWithCurrentFile(options);
					break;
				case Commands.b:
					BackupSramFile(options, SramFileKind.CurrentFile, false);
					break;
				case Commands.r:
					BackupSramFile(options, SramFileKind.CurrentFile, true);
					break;
				case Commands.bc:
					BackupSramFile(options, SramFileKind.ComparisonFile, false);
					break;
				case Commands.rc:
					BackupSramFile(options, SramFileKind.ComparisonFile, true);
					break;
				case Commands.ts:
					TransferSramToOtherGameFile(options);
					break;
				case Commands.ov:
					PrintOffsetValue(options);

					break;
				case Commands.mov:
					SaveOffsetValue(options);

					break;
				case Commands.w:
					Console.Clear();
					break;
				case Commands.q:
					return false;
				default:
					ConsolePrinter.PrintCommands();
					ConsolePrinter.PrintError(Resources.ErrorNoValidCommandCmdTemplate.InsertArgs(command, nameof(Commands.cmd)));

					break;
			}

			return true;
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.Compare{TComparer}(IOptions)"/>
		public virtual void Compare<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			Requires.FileExists(options.ComparisonSramFilepath, nameof(options.ComparisonSramFilepath), Resources.ErrorComparisonFileDoesNotExist);

			using var currFileStream = new FileStream(options.CurrentSramFilepath!, FileMode.Open, FileAccess.Read);
			using var compFileStream = new FileStream(options.ComparisonSramFilepath!, FileMode.Open, FileAccess.Read);

			Compare<TComparer>(currFileStream, compFileStream, options);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.Compare{TComparer}(IOptions, TextWriter)"/>
		public void Compare<TComparer>(IOptions options, TextWriter output) 
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			using (new TemporaryOutputSetter(output))
				Compare<TComparer>(options);

			ConsolePrinter.ResetColor();
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.Compare{TComparer}(Stream, Stream, IOptions, TextWriter)"/>
		public virtual void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options, TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
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

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.Compare{TComparer}(Stream, Stream, IOptions)"/>
		public virtual void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);
			var compFile = ClassFactory.Create<TSramFile>(compStream, options.GameRegion);
			var comparer = ClassFactory.Create<TComparer>(ConsolePrinter);

			comparer.CompareSram(currFile, compFile, options);

			ConsolePrinter.ResetColor();
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.ExportComparison{TComparer}(IOptions)"/>
		public virtual string ExportComparison<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			var srmFilename = Path.GetFileNameWithoutExtension(options.CurrentSramFilepath)!;
			var filename = FilenameHelper.GenerateExportFilename(srmFilename);
			var filepath = Path.Join(options.ExportDirectory, filename);

			ExportComparison<TComparer>(options, filepath);

			return filepath;
		}

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.ExportDirectory"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="showInExplorer">Sets if the file should be selected in windows explorer</param>
		/// /// <returns>The generated filepath</returns>
		public virtual string ExportComparison<TComparer>(IOptions options, bool showInExplorer)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			var filepath = ExportComparison<TComparer>(options);

			if (showInExplorer)
				ExploreFile(filepath);

			return filepath;
		}

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.ExportDirectory"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="filepath"></param>
		/// <param name="showInExplorer">Sets if the file should be selected in windows explorer</param>
		public virtual void ExportComparison<TComparer>(IOptions options, string filepath, bool showInExplorer)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			ExportComparison<TComparer>(options, filepath);

			if (showInExplorer)
				ExploreFile(filepath);
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.ExportComparison{TComparer}(IOptions, string)"/>
		public virtual void ExportComparison<TComparer>(IOptions options, string filepath)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			try
			{
				using var fileStream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
				using var writer = new StreamWriter(fileStream);

				Compare<TComparer>(options, writer);

				writer.Close();
				fileStream.Close();
				ConsolePrinter.PrintParagraph();
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow,
					Resources.StatusCurrentComparisonExportedFilepathTemplate.InsertArgs(filepath));
			}
			catch (Exception ex)
			{
				throw new Exception(Resources.ErrorCannotOpenOutputFileTemplate.InsertArgs(filepath) +
				                    Environment.NewLine + ex.Message);
			}

			ConsolePrinter.ResetColor();
		}

		private static void ExploreFile(string filePath)
		{
			if (!File.Exists(filePath)) return;

			//Clean up file path so it can be navigated OK
			filePath = Path.GetFullPath(filePath);
			Process.Start("explorer.exe", $"/select,\"{filePath}\"");
		}

		public virtual void SaveOffsetValue(IOptions options)
		{
			Requires.FileExists(options.CurrentSramFilepath, nameof(options.CurrentSramFilepath));
			
			var offset = GetOffset(out var slotIndex);
			if (offset == 0)
			{
				ConsolePrinter.PrintError(Resources.ErrorOperationAborted);
				return;
			}

			var value = GetGameOffsetValue();
			var bytes = value switch
			{
				< 256 => new [] { (byte)value },
				< 256 * 256 => BitConverter.GetBytes((ushort)value),
				_ => BitConverter.GetBytes(value),
			};

			var promptResult = InternalGetStringValue(Resources.PromtCreateNewFileInsteadOfOverwriting);
			var createNewFile = promptResult switch
			{
				"1" => true,
				"2" => false,
				_ => throw new OperationCanceledException(),
			};
			
			var saveFilePath = options.CurrentSramFilepath!;
			if (createNewFile)
				saveFilePath += ".manipulated";

			using var currStream = new FileStream(options.CurrentSramFilepath!, FileMode.Open, FileAccess.Read);
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.GameRegion);

			currFile.SetOffsetBytes(slotIndex, offset, bytes);
			currFile.RawSave(saveFilePath);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusSetOffsetValueTemplate.InsertArgs(value, offset));
			var fileName = Path.GetFileName(saveFilePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, createNewFile 
				? Resources.StatusModifiedSramFileHasBeenSavedAsFilepathTemplate.InsertArgs(fileName)
				: Resources.StatusModifiedSramFileHasBeenOverwrittenFilepathTemplate.InsertArgs(fileName));
			ConsolePrinter.ResetColor();
		}

		public virtual void PrintOffsetValue(IOptions options)
		{
			Requires.FileExists(options.CurrentSramFilepath, nameof(options.CurrentSramFilepath));

			using var currStream = new FileStream(options.CurrentSramFilepath!, FileMode.Open, FileAccess.Read);
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

		private int GetOffset(out int slotIndex)
		{
			var promptResult = InternalGetStringValue(Resources.SetSingleSaveSlotMaxTemplate.InsertArgs(4),
				Resources.StatusSetSingleSaveSlotMaxTemplate);
			if (!int.TryParse(promptResult, out var gameId) || gameId > 4)
			{
				ConsolePrinter.PrintError(Resources.ErrorInvalidIndex);
				slotIndex = -1;
				return 0;
			}

			slotIndex = gameId - 1;

			return GetGameOffset(slotIndex);
		}

		public virtual void TransferSramToOtherGameFile(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			var directoryPath = Path.GetDirectoryName(options.CurrentSramFilepath)!;
			var srmFiles = Directory.GetFiles(directoryPath, "*.srm").Where(f => f != options.CurrentSramFilepath).ToArray();
			if (srmFiles.Length == 0)
			{
				ConsolePrinter.PrintLine(Resources.StatusNoAvailableOtherSramFiles);
				return;
			}

			var targetFilepath = GetTargetFilepath();
			if (targetFilepath is null)
				return;

			var targetBackupFilepath = targetFilepath + BackupFileExtension;
			if (!File.Exists(targetBackupFilepath))
			{
				File.Copy(targetFilepath, targetBackupFilepath);
				ConsolePrinter.PrintColored(ConsoleColor.DarkGreen ,Resources.StatusTargetSramFileHasBeenBackedUpFilepathTemplate.InsertArgs(Path.GetFileName(targetBackupFilepath)));
			}

			File.Copy(options.CurrentSramFilepath!, targetFilepath, true);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentSramHasBeenSavedAsFilepathTemplate.InsertArgs(Path.GetFileName(targetFilepath)));

			string? GetTargetFilepath()
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.EnterIndexOfSramFileToBeOverwrittenMaxIndexTemplate.InsertArgs(srmFiles.Length - 1));
				ConsolePrinter.PrintParagraph();
				ConsolePrinter.ResetColor();

				var i = 0;
				foreach (var srmFile in srmFiles)
				{
					ConsolePrinter.PrintColored(ConsoleColor.Cyan, i++.ToString());
					ConsolePrinter.PrintColored(ConsoleColor.White, $@": {Path.GetFileNameWithoutExtension(srmFile)}");
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
			var enumType = flags.GetType();
			var enumFlag = (Enum)Enum.ToObject(enumType, flag);

			flags = EnumHelper.InvertUIntFlag(flags, enumFlag);

			ConsolePrinter.PrintInvertIncludeFlag(flags, enumFlag);

			return flags;
		}

		public int GetGameOffset(int slotIndex) => (int)InternalGetValue(Resources.GetSaveSlotOffsetTemplate.InsertArgs(slotIndex + 1), Resources.StatusOffsetWillBeUsedTemplate);
		public uint GetGameOffsetValue() => InternalGetValue(Resources.GetSaveSlotOffsetValue, Resources.StatusOffsetValueWillBeUsedTemplate);

		private string InternalGetStringValue(string prompt, string? promptResultTemplate = null)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(prompt);

			var input = Console.ReadLine()!;

			ConsolePrinter.PrintParagraph();
			if (promptResultTemplate is not null)
			{
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, promptResultTemplate.InsertArgs(input));

				ConsolePrinter.PrintParagraph();
			}

			ConsolePrinter.ResetColor();
			return input;
		}

		private uint InternalGetValue(string prompt, string promtResultTemplate)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(prompt);

			var input = Console.ReadLine()!;

			uint.TryParse(input, out var offset);

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, promtResultTemplate.InsertArgs(offset));

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.ResetColor();
			return offset;
		}

		public virtual int GetGameId(int maxGameId)
		{
			ConsolePrinter.PrintSectionHeader();
			ConsolePrinter.PrintLine(Resources.SetSaveSlotToCompareMaxTemplate.InsertArgs(maxGameId));

			var input = Console.ReadLine()!;

			int.TryParse(input, out var gameId);

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, gameId == 0
				? string.Format(Resources.StatusGameWillBeComparedTemplate, gameId)
				: Resources.StatusAllGamesWillBeCompared);

			ConsolePrinter.PrintParagraph();
			ConsolePrinter.ResetColor();

			return gameId;
		}

		public virtual void OverwriteComparisonFileWithCurrentFile(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();

			File.Copy(options.CurrentSramFilepath!, options.ComparisonSramFilepath!, true);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentSramFileHasBeenSaved);
			ConsolePrinter.ResetColor();
		}

		public virtual void BackupSramFile(IOptions options, SramFileKind file, bool restore = false)
		{
			var filepath = file == SramFileKind.CurrentFile ? options.CurrentSramFilepath! : options.ComparisonSramFilepath!;
			var fileTypeName = file == SramFileKind.CurrentFile ? Resources.CurrentSramFile : Resources.ComparisonSramFile;
			var backupFilepath = filepath + BackupFileExtension;

			ConsolePrinter.PrintSectionHeader();

			if (restore)
			{
				File.Copy(backupFilepath, filepath, true);
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusSramFileHasBeenRestoredFromBackupTemplate.InsertArgs(fileTypeName));
			}
			else
			{
				File.Copy(filepath, backupFilepath, true);
				ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentSramFileHasBeenBackedUpTemplate.InsertArgs(fileTypeName));
			}

			ConsolePrinter.ResetColor();
		}
	}
}
