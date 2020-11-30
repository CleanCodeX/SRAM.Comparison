using Common.Shared.Min.Extensions;
using SramCommons.Extensions;
using SramComparer.Enums;
using SramComparer.Extensions;
using System;
using System.IO;
using SramComparer.Helpers;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Services
{
	/// <summary>Standard implementation for common print functionality</summary>
	public class ConsolePrinter: IConsolePrinter
	{
		public virtual void PrintSettings(IOptions options)
		{
			PrintSectionHeader();
			PrintColored(ConsoleColor.Gray, Res.Settings + @":");

			PrintSettingName(Res.SettingCurrentGameFilepath, "{0}");
			PrintValue(Path.GetFileName(options.CurrentGameFilepath!));

			PrintSettingName(Res.SettingComparisonGameFilepath, CmdOptions.ComparisonFile);
			PrintValue(Path.GetFileName(options.ComparisonGameFilepath!));

			PrintSettingName(Res.SettingExportDirectory, CmdOptions.Exportdir);
			PrintValue(options.ExportDirectory!);

			PrintSettingName(Res.SettingCurrentGameToCompare, $"{CmdOptions.CurrentGame} [1-4|0={Res.All}]");
			PrintValue(options.CurrentGame == 0 ? Res.All : options.CurrentGame.ToString());

			PrintSettingName(Res.SettingComparisonGameToCompare, $"{CmdOptions.ComparisonGame} [1-4|0={Res.All}]");
			PrintValue(options.ComparisonGame == 0 ? Res.SameAsCurrentGame : options.ComparisonGame.ToString());

			PrintSettingName(Res.SettingRegion, $"{CmdOptions.Region} [{string.Join("|", Enum.GetNames(options.Region.GetType()))}]");
			PrintValue(options.Region.ToString());

			PrintSettingName(Res.ComparisonFlags, $@"{CmdOptions.ComparisonFlags} ""[{string.Join(",", Enum.GetNames(options.Flags.GetType()))}]""");
			PrintValue(Environment.NewLine.PadRight(30) + options.Flags.ToFlagsString());
		}

		protected virtual void PrintCustomCommands() {}

		public virtual void PrintStartMessage()
		{
			var startMessage = @$"== {Res.StartMessage.InsertArgs(nameof(BaseCommands.cmd), nameof(BaseCommands.m))} ==";

			PrintColoredLine(ConsoleColor.DarkYellow, "");
			PrintLine("=".Repeat(startMessage.Length));
			PrintLine(startMessage);
			PrintLine("=".Repeat(startMessage.Length));
		}

		public virtual void PrintCommands()
		{
			PrintSectionHeader();

			PrintGroupName(Res.CmdGroupComparison);

			PrintCommandKey(BaseCommands.c);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandCompareFiles);

			PrintCommandKey(BaseCommands.ow);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandOverwriteComparisonFile);

			PrintGroupName(Res.CmdGroupSetGame);

			PrintCommandKey(BaseCommands.sg);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandSetGame);

			PrintCommandKey(BaseCommands.sgc);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandSetComparisonGame);

			PrintGroupName(Res.CmdGroupBackup);

			PrintCommandKey(BaseCommands.b);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandBackupCurrentFile);

			PrintCommandKey(BaseCommands.bc);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandBackupComparisonFile);

			PrintCommandKey(BaseCommands.r);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandRestoreCurrentFile);

			PrintCommandKey(BaseCommands.rc);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandRestoreComparisonFile);

			PrintCommandKey(BaseCommands.e);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandExportComparisonResult);

			PrintCommandKey(BaseCommands.ts);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandTransferSramToSimilarGameFile);

			PrintGroupName(Res.CmdGroupDisplay);

			PrintCommandKey(BaseCommands.m);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandManual);

			PrintCommandKey(BaseCommands.cmd);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandDisplayCommands);

			PrintCommandKey(BaseCommands.s);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandDisplaySettings);

			PrintCommandKey(BaseCommands.w);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandWipeOutput);

			PrintGroupName(Res.CmdOther);

			PrintCommandKey(BaseCommands.fwg);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandIncludeWholeGameBufferComparison);

			PrintCommandKey(BaseCommands.fng);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandIncludeNonGameBufferComparison);

			PrintCommandKey(BaseCommands.pov);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandSaveOffsetValue);

			PrintCommandKey(BaseCommands.sov);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandSaveOffsetValue);

			PrintCustomCommands();

			PrintParagraph();
			PrintParagraph();
			PrintCommandKey(BaseCommands.q);
			PrintColoredLine(ConsoleColor.Yellow, Res.CommandQuit);
			PrintParagraph();

			PrintColoredLine(ConsoleColor.Cyan, Res.EnterCommand);
			ResetColor();
		}

		protected virtual void PrintGroupName(string groupName) => PrintColoredLine(ConsoleColor.DarkGray, Environment.NewLine + groupName);

		protected virtual void PrintCommandKey(Enum key) => PrintColored(ConsoleColor.White, @$"{key,12}: ");

		protected virtual string GetManualText() => Res.AppManualCommandsTemplate.InsertArgs(
			BaseCommands.ow, BaseCommands.c, BaseCommands.e, BaseCommands.sg, BaseCommands.sgc, BaseCommands.fwg, BaseCommands.fng,
			BaseCommands.b, BaseCommands.bc, BaseCommands.r, BaseCommands.rc);

		protected virtual string GetAppDescriptionText() => Res.AppDescription;

		public virtual void PrintManual()
		{
			PrintSectionHeader();
			PrintColoredLine(ConsoleColor.Yellow, GetAppDescriptionText());
			PrintParagraph();
			PrintColoredLine(ConsoleColor.Cyan, GetManualText());
			ResetColor();
		}

		public virtual void PrintInvertIncludeFlag(Enum flags, Enum flag)
		{
			PrintSectionHeader();
			Print(flag + @":");
			PrintColoredLine(ConsoleColor.Yellow, @" " + flags.HasFlag(flag));
			ResetColor();
		}

		protected virtual void PrintCustomBufferInfo() {}
		public virtual void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength)
		{
			PrintParagraph();

			PrintColored(ConsoleColor.Gray, " ".Repeat(4) + @$"[ {Res.Section} ");

			PrintColored(ConsoleColor.DarkYellow, bufferName);
			PrintColored(ConsoleColor.White, @" | ");
			PrintColored(ConsoleColor.Gray, $@"{Res.Offset} ");
			PrintColored(ConsoleColor.DarkYellow, bufferOffset + $@" [x{bufferOffset:X}]");

			PrintColored(ConsoleColor.White, @" | ");

			PrintColored(ConsoleColor.Gray, $@"{Res.Size} ");
			PrintColored(ConsoleColor.DarkYellow, bufferLength.ToString());

			PrintColored(ConsoleColor.White, @" ]");

			PrintCustomBufferInfo();

			PrintParagraph();
			ResetColor();
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, ushort currValue, ushort compValue)
		{
			var sign = GetNumberSign((short)(currValue - compValue));
			var change = currValue - compValue;
			var absChange = (uint)Math.Abs(change);
			var changeString = $"{absChange,5:###}";
			var isNegativechange = change < 0;

			var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
			var compText = $"{compValue,3:D5} [x{compValue,2:X4}] [{compValue.FormatBinary(16)}]";
			var currText = $"{currValue,3:D5} [x{currValue,2:X4}] [{currValue.FormatBinary(16)}]";
			var changeText = $"{changeString,5} [x{absChange,2:X4}] [{absChange.FormatBinary(16)}]";

			PrintComparisonIdentification(ident);
			PrintOffsetValues(offsetText, offsetName);
			PrintCompValues(isNegativechange, compText);
			PrintCurrValues(isNegativechange, currText);
			PrintChangeValues(isNegativechange, absChange, sign, changeText);
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue)
		{
			var sign = GetNumberSign((short)(currValue - compValue));
			var change = currValue - compValue;
			var absChange = (uint)Math.Abs(change);
			var isNegativechange = change < 0;

			var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
			var compText = GetByteValueRepresentations(compValue);
			var currText = GetByteValueRepresentations(currValue);
			var changeText = GetByteValueChangeRepresentations(currValue, compValue);

			PrintComparisonIdentification(ident);
			PrintOffsetValues(offsetText, offsetName);
			PrintCompValues(isNegativechange, compText);
			PrintCurrValues(isNegativechange, currText);
			PrintChangeValues(isNegativechange, absChange, sign, changeText);
		}

		protected virtual string GetByteValueRepresentations(byte value) =>
			NumberFormatter.GetByteValueRepresentations(value);

		protected virtual string GetByteValueChangeRepresentations(byte currValue, byte compValue) =>
			NumberFormatter.GetByteValueChangeRepresentations(currValue, compValue);

		public virtual void PrintSectionHeader()
		{
			ResetColor();
			PrintParagraph();
			PrintColoredLine(ConsoleColor.Cyan, $@"===[ {DateTime.Now.ToLongTimeString()} ]====================================================");
			ResetColor();
		}

		public virtual void PrintError(string message)
		{
			PrintParagraph();
			PrintColoredLine(ConsoleColor.Red, message);
			ResetColor();
		}

		public virtual void PrintError(Exception ex)
		{
			PrintParagraph();
			PrintColoredLine(ConsoleColor.Red, ex.Message);
			ResetColor();
		}

		public virtual void PrintFatalError(string fataError)
		{
			PrintParagraph();
			
			PrintColoredLine(ConsoleColor.Yellow, ConsoleColor.Red, fataError);
			ResetColor();
		}

		protected virtual void PrintSettingName(string settingName, string? cmdArg = null)
		{
			PrintColored(ConsoleColor.White, settingName.PadRight(28) + @":");

			if (cmdArg is null) return;

			PrintColored(ConsoleColor.Cyan, cmdArg);
		}

		protected virtual void PrintValue(object value) => PrintColoredLine(ConsoleColor.Yellow, @" " + value);

		protected virtual void PrintOffsetValues(string offsetText, string? offsetName)
		{
			PrintColored(ConsoleColor.DarkGray, $@"{Res.Offset} ");
			PrintColored(ConsoleColor.DarkCyan, offsetText);

			if (offsetName is null) return;

			PrintColored(ConsoleColor.Cyan, @" => ");
			PrintColored(ConsoleColor.DarkYellow, offsetName);
		}

		protected virtual void PrintCompValues(bool isNegativeChange, string compText)
		{
			PrintColored(ConsoleColor.White, @" | ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompShort} ");
			PrintColored(isNegativeChange ? ConsoleColor.DarkGreen : ConsoleColor.Red, compText);
		}

		protected virtual void PrintCurrValues(bool isNegativeChange, string currText)
		{
			PrintColored(ConsoleColor.Cyan, @" => ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CurrShort} ");
			PrintColored(isNegativeChange ? ConsoleColor.Red : ConsoleColor.DarkGreen, currText);
		}

		protected virtual void PrintChangeValues(bool isNegativeChange, uint changeValue, string sign, string changeText)
		{
			PrintColored(ConsoleColor.Cyan, @" = ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.ChangeShort} ");

			PrintColored(isNegativeChange ? ConsoleColor.DarkRed : ConsoleColor.Green, sign);
			PrintColored(isNegativeChange ? ConsoleColor.Red : ConsoleColor.DarkGreen, changeText);

			var changedBits = changeValue.CountChangedBits();

			PrintColored(ConsoleColor.DarkGray, ":");

			PrintColoredLine(changedBits switch
			{
				1 => ConsoleColor.Magenta,
				8 => ConsoleColor.Yellow,
				_ => ConsoleColor.Gray
			}, changedBits.ToString());

			ResetColor();
		}

		protected virtual string GetNumberSign(short value) => Math.Sign(value) < 0 ? "(-)" : "(+)";

		protected virtual void PrintComparisonIdentification(string ident) => PrintColored(ConsoleColor.White, $@"{ident}=> ");

		protected virtual void PrintColored(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text)
		{
			SetForegroundColor(backgroundColor);
			PrintColored(foregroundColor, text);
		}

		public virtual void PrintColored(ConsoleColor color, string text)
		{
			SetForegroundColor(color);
			Print(text);
		}

		public virtual void PrintColoredLine(ConsoleColor color, string text)
		{
			SetForegroundColor(color);
			PrintLine(text);
		}

		public virtual void ResetColor() => Console.ResetColor();
		public virtual void PrintParagraph() => Console.WriteLine();
		public virtual void Print(string text) => Console.Write(text);
		public virtual void PrintLine(string text) => Console.WriteLine(text);

		protected virtual void PrintBackgroundColored(ConsoleColor color, string text)
		{
			SetBackgroundColor(color);
			Print(text);
		}

		protected virtual void PrintBackgroundColoredLine(ConsoleColor color, string text)
		{
			SetBackgroundColor(color);
			PrintLine(text);
		}

		protected virtual void PrintColoredLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text)
		{
			SetBackgroundColor(foregroundColor);
			PrintColoredLine(foregroundColor, text);
		}
		
		protected virtual void SetForegroundColor(ConsoleColor color) => Console.ForegroundColor = color;
		protected virtual void SetBackgroundColor(ConsoleColor color) => Console.BackgroundColor = color;
	}
}