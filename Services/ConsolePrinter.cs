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
			PrintColoredLine(ConsoleColor.Gray, Res.Settings + @":");

			PrintSettingName(Res.CurrentSramFilepath, "{0}");
			PrintValue(Path.GetFileName(options.CurrentSramFilePath!));

			PrintSettingName(Res.ComparisonSramFilepath, "{1-2}|" + CmdOptions.ComparisonFile);
			PrintValue(Path.GetFileName(options.ComparisonSramFilePath!));

			PrintSettingName(Res.GameRegion, $"{"{1-2}|" + CmdOptions.GameRegion} [{string.Join("|", Enum.GetNames(options.GameRegion.GetType()))}]");
			PrintValue(options.GameRegion.ToString());

			PrintSettingName(Res.ExportDirectory, CmdOptions.ExportDirectory);
			PrintValue(options.ExportDirectory!);

			PrintSettingName(Res.CurrentSramFileSaveSlot, $"{CmdOptions.CurrentSaveSlot} [1-4|0={Res.All}]");
			PrintValue(options.CurrentSramFileSaveSlot == 0 ? Res.All : options.CurrentSramFileSaveSlot.ToString());

			PrintSettingName(Res.ComparisonSramFileSaveSlot, $"{CmdOptions.ComparisonSaveSlot} [1-4|0={Res.All}]");
			PrintValue(options.ComparisonSramFileSaveSlot == 0 ? Res.SameAsCurrentSramFileSaveSlot : options.ComparisonSramFileSaveSlot.ToString());

			PrintSettingName(Res.ColorizeOutput, $"{CmdOptions.ColorizeOutput} [true|1|false|0]");
			PrintValue(options.ColorizeOutput.ToString());

			PrintSettingName(Res.UILanguage, CmdOptions.UILanguage);
			PrintValue(options.UILanguage!);

			PrintSettingName(Res.ComparisonResultLanguage, CmdOptions.ComparisonResultLanguage);
			PrintValue(options.ComparisonResultLanguage!);

			PrintSettingName(Res.ComparisonFlags, $@"{CmdOptions.ComparisonFlags} [{string.Join(",", Enum.GetNames(options.ComparisonFlags.GetType()))}]");
			PrintSettingName(Environment.NewLine, padRightDistance: 37);
			PrintValue(options.ComparisonFlags.ToFlagsString());
		}

		protected virtual void PrintCustomCommands() {}

		public virtual void PrintStartMessage()
		{
			PrintSectionHeader();
			PrintLine("");
			PrintColoredLine(ConsoleColor.Yellow, GetAppDescriptionText());

			var startMessage = @$"== {Res.StartMessage.InsertArgs(nameof(Commands.cmd), nameof(Commands.g_srm))} ==";

			PrintColoredLine(ConsoleColor.DarkYellow, "");
			PrintLine("=".Repeat(startMessage.Length));
			PrintLine(startMessage);
			PrintLine("=".Repeat(startMessage.Length));
		}

		public virtual void PrintCommands()
		{
			PrintSectionHeader();

			PrintGroupName(Res.CmdGroupComparison);

			PrintCommandKey(Commands.c);
			PrintColoredLine(ConsoleColor.Yellow, Commands.c.GetDisplayName()!);

			PrintCommandKey(Commands.ow);
			PrintColoredLine(ConsoleColor.Yellow, Commands.ow.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupSetsSaveSlot);

			PrintCommandKey(Commands.ss);
			PrintColoredLine(ConsoleColor.Yellow, Commands.ss.GetDisplayName()!);

			PrintCommandKey(Commands.ssc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.ssc.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupBackup);

			PrintCommandKey(Commands.b);
			PrintColoredLine(ConsoleColor.Yellow, Commands.b.GetDisplayName()!);

			PrintCommandKey(Commands.bc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.bc.GetDisplayName()!);

			PrintCommandKey(Commands.r);
			PrintColoredLine(ConsoleColor.Yellow, Commands.r.GetDisplayName()!);

			PrintCommandKey(Commands.rc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.rc.GetDisplayName()!);

			PrintCommandKey(Commands.e);
			PrintColoredLine(ConsoleColor.Yellow, Commands.e.GetDisplayName()!);

			PrintCommandKey(Commands.ts);
			PrintColoredLine(ConsoleColor.Yellow, Commands.ts.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupDisplay);

			PrintCommandKey(Commands.g_srm);
			PrintColoredLine(ConsoleColor.Yellow, Commands.g_srm.GetDisplayName()!);

			PrintCommandKey(Commands.g_savestate);
			PrintColoredLine(ConsoleColor.Yellow, Commands.g_savestate.GetDisplayName()!);

			PrintCommandKey(Commands.cmd);
			PrintColoredLine(ConsoleColor.Yellow, Commands.cmd.GetDisplayName()!);

			PrintCommandKey(Commands.s);
			PrintColoredLine(ConsoleColor.Yellow, Commands.s.GetDisplayName()!);

			PrintCommandKey(Commands.w);
			PrintColoredLine(ConsoleColor.Yellow, Commands.w.GetDisplayName()!);

			PrintGroupName(Res.CmdMisc);

			PrintCommandKey(Commands.asbc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.asbc.GetDisplayName()!);

			PrintCommandKey(Commands.nsbc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.nsbc.GetDisplayName()!);

			PrintCommandKey(Commands.ov);
			PrintColoredLine(ConsoleColor.Yellow, Commands.ov.GetDisplayName()!);

			PrintCommandKey(Commands.mov);
			PrintColoredLine(ConsoleColor.Yellow, Commands.mov.GetDisplayName()!);

			PrintGroupName(Res.CmdLanguage);

			PrintCommandKey(Commands.l);
			PrintColoredLine(ConsoleColor.Yellow, Commands.l.GetDisplayName()!);

			PrintCommandKey(Commands.lc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.lc.GetDisplayName()!);

			PrintCustomCommands();

			PrintParagraph();
			PrintParagraph();
			PrintCommandKey(Commands.q);
			PrintColoredLine(ConsoleColor.Yellow, Commands.q.GetDisplayName()!);
			PrintParagraph();

			PrintColoredLine(ConsoleColor.Cyan, Res.EnterCommand);
			ResetColor();
		}

		protected virtual void PrintGroupName(string groupName) => PrintColoredLine(ConsoleColor.DarkGray, Environment.NewLine + groupName);

		protected virtual void PrintCommandKey(Enum key) => PrintColored(ConsoleColor.White, @$"{key,12}: ");

		protected virtual string GetGuideText(string? guideName) => Res.NoGuideAvailable;

		protected virtual string GetAppDescriptionText() => Res.AppDescription;

		public virtual void PrintGuide(string? guideName)
		{
			PrintParagraph();
			PrintColoredLine(ConsoleColor.Cyan, GetGuideText(guideName));
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

		protected virtual void PrintSettingName(string settingName, string? cmdArg = null, int padRightDistance = 35)
		{
			PrintColored(ConsoleColor.White, settingName.PadRight(padRightDistance) + @":");

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
			PrintColored(ConsoleColor.DarkGray, $@"{Res.Old} ");
			PrintColored(isNegativeChange ? ConsoleColor.DarkGreen : ConsoleColor.Red, compText);
		}

		protected virtual void PrintCurrValues(bool isNegativeChange, string currText)
		{
			PrintColored(ConsoleColor.Cyan, @" => ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.New} ");
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

		public virtual void ResetColor()
		{
			if (!ColorizeOutput) return;
			
			Console.ResetColor();
		}

		public bool ColorizeOutput { get; set; } = true;
		public virtual string NewLine => Environment.NewLine;
		
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
			SetBackgroundColor(backgroundColor);
			PrintColoredLine(foregroundColor, text);
		}
		
		protected virtual void SetForegroundColor(ConsoleColor color)
		{
			if (!ColorizeOutput) return;
			
			Console.ForegroundColor = color;
		}

		protected virtual void SetBackgroundColor(ConsoleColor color)
		{
			if (!ColorizeOutput) return;
			
			Console.BackgroundColor = color;
		}
	}
}