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

			if (options.CurrentGameFilepath.IsNullOrEmpty())
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
					options.CurrentGame = GetGameId(maxGameId: 4);
					if (options.CurrentGame == default)
						options.ComparisonGame = default;

					break;
				case nameof(BaseCommands.sgc):
					if (options.CurrentGame != default)
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
				case nameof(BaseCommands.dov):
					PrintOffsetValue(options);

					break;
				case nameof(BaseCommands.mov):
					SaveOffsetValue(options);

					break;
				case nameof(BaseCommands.w):
					Console.Clear();
					break;
				case nameof(BaseCommands.q):
					return false;
				default:
					ConsolePrinter.PrintCommands();
					ConsolePrinter.PrintError(Resources.ErrorNoValidCommandCmdTemplate.InsertArgs(command, nameof(BaseCommands.cmd)));

					break;
			}

			return true;
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.Compare{TComparer}(IOptions)"/>
		public virtual void Compare<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			Requires.FileExists(options.ComparisonGameFilepath, nameof(options.ComparisonGameFilepath), Resources.ErrorComparisonFileDoesNotExist);

			var currFileStream = new FileStream(options.CurrentGameFilepath!, FileMode.Open, FileAccess.Read);
			var compFileStream = new FileStream(options.ComparisonGameFilepath!, FileMode.Open, FileAccess.Read);

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
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.Region);
			var compFile = ClassFactory.Create<TSramFile>(compStream, options.Region);
			var comparer = ClassFactory.Create<TComparer>(ConsolePrinter);

			comparer.CompareSram(currFile, compFile, options);

			ConsolePrinter.ResetColor();
		}

		/// <inheritdoc cref="ICommandHandler{TSramFile,TSramGame}.ExportComparison{TComparer}(IOptions)"/>
		public virtual string ExportComparison<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSramGame>, new()
		{
			var srmFilename = Path.GetFileNameWithoutExtension(options.CurrentGameFilepath)!;
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

				Compare<TComparer>(options, new StreamWriter(fileStream));

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
			Requires.FileExists(options.CurrentGameFilepath, nameof(options.CurrentGameFilepath));
			
			var offset = GetOffset(out var gameIndex);
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
			
			var saveFilePath = options.CurrentGameFilepath!;
			if (createNewFile)
				saveFilePath += ".manipulated";

			var currStream = new FileStream(options.CurrentGameFilepath!, FileMode.Open, FileAccess.Read);
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.Region);

			currFile.SetOffsetBytes(gameIndex, offset, bytes);
			currFile.RawSave(saveFilePath);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusSetOffsetValueTemplate.InsertArgs(value, offset));
			var fileName = Path.GetFileName(saveFilePath);
			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, createNewFile 
				? Resources.StatusChangedSramFileHasBeenSavedAsFilepathTemplate.InsertArgs(fileName)
				: Resources.StatusChangedSramFileHasBeenOverwrittenFilepathTemplate.InsertArgs(fileName));
			ConsolePrinter.ResetColor();
		}

		public virtual void PrintOffsetValue(IOptions options)
		{
			Requires.FileExists(options.CurrentGameFilepath, nameof(options.CurrentGameFilepath));

			var currStream = new FileStream(options.CurrentGameFilepath!, FileMode.Open, FileAccess.Read);
			var currFile = ClassFactory.Create<TSramFile>(currStream, options.Region);

			var offset = GetOffset(out var gameIndex);
			if (offset == 0)
			{
				ConsolePrinter.PrintError(Resources.ErrorOperationAborted);
				return;
			}
			
			var byteValue = currFile.GetOffsetByte(gameIndex, offset);

			var valueDisplayText = NumberFormatter.GetByteValueRepresentations(byteValue);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Green, Resources.StatusGetOffsetValueTemplate.InsertArgs(offset, valueDisplayText));
			ConsolePrinter.ResetColor();
		}

		private int GetOffset(out int gameIndex)
		{
			var promptResult = InternalGetStringValue(Resources.SetSingleGameMaxTemplate.InsertArgs(4),
				Resources.StatusSetSingleGameMaxTemplate);
			if (!int.TryParse(promptResult, out var gameId) || gameId > 4)
			{
				ConsolePrinter.PrintError(Resources.ErrorInvalidIndex);
				gameIndex = -1;
				return 0;
			}

			gameIndex = gameId - 1;

			return GetGameOffset(gameIndex);
		}

		public virtual void TransferSramToOtherGameFile(IOptions options)
		{
			ConsolePrinter.PrintSectionHeader();
			var directoryPath = Path.GetDirectoryName(options.CurrentGameFilepath)!;
			var srmFiles = Directory.GetFiles(directoryPath, "*.srm").Where(f => f != options.CurrentGameFilepath).ToArray();
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

			File.Copy(options.CurrentGameFilepath!, targetFilepath, true);
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

		public int GetGameOffset(int gameIndex) => (int)InternalGetValue(Resources.SetGameOffsetTemplate.InsertArgs(gameIndex + 1), Resources.StatusOffsetWillBeUsedTemplate);
		public uint GetGameOffsetValue() => InternalGetValue(Resources.SetGameOffsetValue, Resources.StatusOffsetValueWillBeUsedTemplate);

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
			ConsolePrinter.PrintLine(Resources.SetGameToCompareMaxTemplate.InsertArgs(maxGameId));

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

			File.Copy(options.CurrentGameFilepath!, options.ComparisonGameFilepath!, true);

			ConsolePrinter.PrintColoredLine(ConsoleColor.Yellow, Resources.StatusCurrentSramFileHasBeenSaved);
			ConsolePrinter.ResetColor();
		}

		public virtual void BackupSramFile(IOptions options, SramFileKind file, bool restore = false)
		{
			var filepath = file == SramFileKind.Current ? options.CurrentGameFilepath : options.ComparisonGameFilepath;
			var fileTypeName = file == SramFileKind.Current ? Resources.CurrentSramFile : Resources.ComparisonSramFile;
			var backupFilepath = filepath += BackupFileExtension;

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
