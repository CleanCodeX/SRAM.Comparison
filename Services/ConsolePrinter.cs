using Common.Shared.Min.Extensions;
using SramCommons.Extensions;
using SramComparer.Enums;
using SramComparer.Extensions;
using System;
using System.IO;
using System.Linq;
using SramComparer.Helpers;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Services
{
	/// <summary>Standard implementation for common print functionality</summary>
	public class ConsolePrinter: IConsolePrinter
	{
		public virtual void PrintConfig(IOptions options)
		{
			PrintSectionHeader();
			PrintColoredLine(ConsoleColor.Gray, Res.Config + @":");

			PrintConfigLine(Res.ConfigCurrentFilePath, "{0}", Path.GetFileName(options.CurrentFilePath!));
			PrintConfigLine(Res.ConfigComparisonFilePath, "{1-2}|" + CmdOptions.ComparisonFile, Path.GetFileName(FileNameHelper.GetComparisonFilePath(options)));

			PrintConfigLine(Res.EnumGameRegion, $"{"{1-2}|" + CmdOptions.GameRegion} [{string.Join("|", Enum.GetNames(options.GameRegion.GetType()))}]", options.GameRegion.ToString());

			PrintConfigLine(Res.ConfigExportDirectory, CmdOptions.ExportDirectory, options.ExportDirectory ?? Path.GetDirectoryName(options.CurrentFilePath)!);

			PrintConfigLine(Res.ConfigCurrentFileSaveSlot, $"{CmdOptions.CurrentSaveSlot} [1-4|0={Res.All}]", options.CurrentFileSaveSlot == 0 ? Res.All : options.CurrentFileSaveSlot.ToString());

			PrintConfigLine(Res.ConfigComparisonFileSaveSlot, $"{CmdOptions.ComparisonSaveSlot} [1-4|0={Res.All}]", options.ComparisonFileSaveSlot == 0 ? Res.CompSameAsCurrentFileSaveSlot : options.ComparisonFileSaveSlot.ToString());

			PrintConfigLine(Res.ConfigColorizeOutput, $"{CmdOptions.ColorizeOutput} [true|1|false|0]", options.ColorizeOutput.ToString());
			PrintConfigLine(Res.ConfigUILanguage, CmdOptions.UILanguage, options.UILanguage!);
			PrintConfigLine(Res.ConfigComparisonResultLanguage, CmdOptions.ComparisonResultLanguage, options.ComparisonResultLanguage!);
			PrintConfigLine(Res.ConfigFilePath, CmdOptions.ConfigFilePath, options.ConfigFilePath!);

			PrintConfigName(Res.ConfigComparisonFlags, $@"{CmdOptions.ComparisonFlags} [{string.Join(",", Enum.GetNames(options.ComparisonFlags.GetType()))}]");
			PrintConfigName(Environment.NewLine, padRightDistance: 37);
			PrintValue(options.ComparisonFlags.ToFlagsString());
		}

		public void PrintConfigLine(string name, string value) => PrintConfigLine(name, null!, value);
		public void PrintConfigLine(string name, string args, string value)
		{
			PrintConfigName(name, args);
			PrintValue(value);
		}

		public void Clear() => Console.Clear();

		protected virtual void PrintCustomCommands() {}

		public virtual void PrintStartMessage()
		{
			PrintSectionHeader();
			PrintLine("");
			PrintColoredLine(ConsoleColor.Yellow, GetAppDescriptionText());

			var startMessage = @$"== {Res.StartMessage.InsertArgs(nameof(Commands.Help), nameof(Commands.Guide_Srm))} ==";

			PrintColoredLine(ConsoleColor.DarkYellow, "");
			PrintLine("=".Repeat(startMessage.Length));
			PrintLine(startMessage);
			PrintLine("=".Repeat(startMessage.Length));
			ResetColor();
		}

		public virtual void PrintCommands()
		{
			PrintSectionHeader();

			PrintGroupName(Res.CmdGroupComparison);

			PrintCommandKey(Commands.Compare);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Compare.GetDisplayName()!);

			PrintCommandKey(Commands.OverwriteComp);
			PrintColoredLine(ConsoleColor.Yellow, Commands.OverwriteComp.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupSetsSaveSlot);

			PrintCommandKey(Commands.SetSlot);
			PrintColoredLine(ConsoleColor.Yellow, Commands.SetSlot.GetDisplayName()!);

			PrintCommandKey(Commands.SetSlot_Comp);
			PrintColoredLine(ConsoleColor.Yellow, Commands.SetSlot_Comp.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupBackup);

			PrintCommandKey(Commands.Backup);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Backup.GetDisplayName()!);

			PrintCommandKey(Commands.Backup_Comp);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Backup_Comp.GetDisplayName()!);

			PrintCommandKey(Commands.Restore);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Restore.GetDisplayName()!);

			PrintCommandKey(Commands.Restore_Comp);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Restore_Comp.GetDisplayName()!);

			PrintCommandKey(Commands.Export);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Export.GetDisplayName()!);

			PrintCommandKey(Commands.Transfer);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Transfer.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupDisplay);

			PrintCommandKey(Commands.Guide_Srm);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Guide_Srm.GetDisplayName()!);

			PrintCommandKey(Commands.Guide_Savestate);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Guide_Savestate.GetDisplayName()!);

			PrintCommandKey(Commands.Help);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Help.GetDisplayName()!);

			PrintCommandKey(Commands.Config);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Config.GetDisplayName()!);

			PrintCommandKey(Commands.Clear);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Clear.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupMisc);

			PrintCommandKey(Commands.Sbc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Sbc.GetDisplayName()!);

			PrintCommandKey(Commands.Nsbc);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Nsbc.GetDisplayName()!);

			PrintCommandKey(Commands.Offset);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Offset.GetDisplayName()!);

			PrintCommandKey(Commands.EditOffset);
			PrintColoredLine(ConsoleColor.Yellow, Commands.EditOffset.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupLanguage);

			PrintCommandKey(Commands.Lang);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Lang.GetDisplayName()!);

			PrintCommandKey(Commands.Lang_Comp);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Lang_Comp.GetDisplayName()!);

			PrintGroupName(Res.CmdGroupConfig);

			PrintCommandKey(Commands.LoadConfig);
			PrintColoredLine(ConsoleColor.Yellow, Commands.LoadConfig.GetDisplayName()!);

			PrintCommandKey(Commands.SaveConfig);
			PrintColoredLine(ConsoleColor.Yellow, Commands.SaveConfig.GetDisplayName()!);

			PrintCommandKey(Commands.OpenConfig);
			PrintColoredLine(ConsoleColor.Yellow, Commands.OpenConfig.GetDisplayName()!);

			PrintCommandKey(Commands.AutoLoadOn);
			PrintColoredLine(ConsoleColor.Yellow, Commands.AutoLoadOn.GetDisplayName()!);

			PrintCommandKey(Commands.AutoLoadOff);
			PrintColoredLine(ConsoleColor.Yellow, Commands.AutoLoadOff.GetDisplayName()!);

			PrintCommandKey(Commands.CreateBindings);
			PrintColoredLine(ConsoleColor.Yellow, Commands.CreateBindings.GetDisplayName()!);

			PrintCommandKey(Commands.OpenBindings);
			PrintColoredLine(ConsoleColor.Yellow, Commands.OpenBindings.GetDisplayName()!);

			PrintCustomCommands();

			PrintLine();
			PrintLine();
			PrintCommandKey(Commands.Quit);
			PrintColoredLine(ConsoleColor.Yellow, Commands.Quit.GetDisplayName()!);
			PrintLine();

			PrintColoredLine(ConsoleColor.Cyan, Res.EnterCommand);
			ResetColor();
		}

		protected virtual void PrintGroupName(string groupName) => PrintColoredLine(ConsoleColor.DarkGray, Environment.NewLine + groupName);

		protected virtual void PrintCommandKey(Enum key) => PrintColored(ConsoleColor.White, @$"{key + GetAlternateCommands(key),25}: ");

		private static string GetAlternateCommands(Enum cmd)
		{
			var compValue = cmd.ToInt();
			var dict = default(AlternateCommands).ToDictionary();
			var altKeys = dict
				.Where(e => e.Value.ToInt() == compValue)
				.Select(e => e.Key).ToArray();

			return altKeys.Length > 0 ? $" [{altKeys.Join()}]" : string.Empty;
		}

		protected virtual string GetGuideText(string? guideName) => Res.StatusNoGuideAvailable;

		protected virtual string GetAppDescriptionText() => Res.AppDescription;

		public virtual void PrintGuide(string? guideName)
		{
			PrintLine();
			PrintColoredLine(ConsoleColor.Cyan, GetGuideText(guideName));
			ResetColor();
		}

		public virtual void PrintInvertIncludeFlag(in Enum flags, in Enum flag)
		{
			PrintSectionHeader();
			Print(flag + @":");
			PrintColoredLine(ConsoleColor.Yellow, @" " + flags.HasFlag(flag));
			ResetColor();
		}

		protected virtual void PrintCustomBufferInfo() {}
		public virtual void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength)
		{
			PrintLine();

			PrintColored(ConsoleColor.Gray, " ".Repeat(4) + @$"[ {Res.CompSection} ");

			PrintColored(ConsoleColor.DarkYellow, bufferName);
			PrintColored(ConsoleColor.White, @" | ");
			PrintColored(ConsoleColor.Gray, $@"{Res.CompOffset} ");
			PrintColored(ConsoleColor.DarkYellow, bufferOffset + $@" [x{bufferOffset:X}]");

			PrintColored(ConsoleColor.White, @" | ");

			PrintColored(ConsoleColor.Gray, $@"{Res.CompSize} ");
			PrintColored(ConsoleColor.DarkYellow, bufferLength.ToString());

			PrintColored(ConsoleColor.White, @" ]");

			PrintCustomBufferInfo();

			PrintLine();
			ResetColor();
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, uint currValue, uint compValue)
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

		public virtual void PrintComparison(string ident, int offset, string? offsetName, ushort currValue,
			ushort compValue) => PrintComparison(ident, offset, offsetName, (uint)currValue, compValue);

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
			PrintLine();
			PrintColoredLine(ConsoleColor.Cyan, $@"===[ {DateTime.Now.ToLongTimeString()} ]====================================================");
			ResetColor();
		}

		public virtual void PrintError(string message)
		{
			PrintLine();
			PrintColoredLine(ConsoleColor.Red, message);
			ResetColor();
		}

		public virtual void PrintError(Exception ex)
		{
			PrintLine();
			PrintColoredLine(ConsoleColor.Red, ex.Message);
			ResetColor();
		}

		public virtual void PrintFatalError(string fataError)
		{
			PrintLine();
			
			PrintColoredLine(ConsoleColor.Yellow, ConsoleColor.DarkRed, fataError);
			ResetColor();
		}

		protected virtual void PrintConfigName(string settingName, string? cmdArg = null, int padRightDistance = 35)
		{
			PrintColored(ConsoleColor.White, settingName.PadRight(padRightDistance) + @":");

			if (cmdArg is null) return;

			PrintColored(ConsoleColor.Cyan, cmdArg);
		}

		protected virtual void PrintValue(object value) => PrintColoredLine(ConsoleColor.Yellow, @" " + value);

		protected virtual void PrintOffsetValues(string offsetText, string? offsetName)
		{
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompOffset} ");
			PrintColored(ConsoleColor.DarkCyan, offsetText);

			if (offsetName is null) return;

			PrintColored(ConsoleColor.Cyan, @" => ");
			PrintColored(ConsoleColor.DarkYellow, offsetName);
		}

		protected virtual void PrintCompValues(bool isNegativeChange, string compText)
		{
			PrintColored(ConsoleColor.White, @" | ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompOld} ");
			PrintColored(isNegativeChange ? ConsoleColor.DarkGreen : ConsoleColor.Red, compText);
		}

		protected virtual void PrintCurrValues(bool isNegativeChange, string currText)
		{
			PrintColored(ConsoleColor.Cyan, @" => ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompNew} ");
			PrintColored(isNegativeChange ? ConsoleColor.Red : ConsoleColor.DarkGreen, currText);
		}

		protected virtual void PrintChangeValues(bool isNegativeChange, uint changeValue, string sign, string changeText)
		{
			PrintColored(ConsoleColor.Cyan, @" = ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompChangeShort} ");

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
		
		public virtual void PrintLine() => Console.WriteLine();
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