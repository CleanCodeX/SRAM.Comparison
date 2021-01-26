using System;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using IO.Extensions;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Extensions;
using SRAM.Comparison.Helpers;
using Res = SRAM.Comparison.Properties.Resources;

namespace SRAM.Comparison.Services
{
	/// <summary>Standard implementation for common print functionality</summary>
	public class ConsolePrinter: IConsolePrinter
	{
		private const string Prefix = " ";
		private static readonly string NewLineDefault = Environment.NewLine;

		public virtual void PrintConfig(IOptions options)
		{
			PrintSectionHeader();
			PrintColoredLine(ConsoleColor.Gray, Res.Config + @":");

			PrintConfigLine(Res.ConfigCurrentFilePath, "{0}", Path.GetFileName(options.CurrentFilePath!));
			PrintConfigLine(Res.ConfigComparisonFilePath, "{1-2}|" + CmdOptions.ComparisonPath, Path.GetFileName(FilePathHelper.GetComparisonFilePath(options)));

			PrintConfigLine(Res.EnumGameRegion, $"{"{1-2}|" + CmdOptions.GameRegion} [{string.Join("|", Enum.GetNames(options.GameRegion.GetType()))}]", options.GameRegion.ToString());

			PrintConfigName(Res.ConfigComparisonFlags, $@"{CmdOptions.ComparisonFlags} [{string.Join(",", Enum.GetNames(options.ComparisonFlags.GetType()))}]");
			PrintConfigName(Environment.NewLine, padRightDistance: 37);
			PrintValue(options.ComparisonFlags.ToFlagsString());

			PrintConfigLine(Res.ConfigExportDirectory, CmdOptions.ExportPath, options.ExportPath ?? Path.GetDirectoryName(options.CurrentFilePath)!);

			PrintConfigName(Res.ConfigExportFlags, $@"{CmdOptions.ExportFlags} [{string.Join(",", Enum.GetNames(options.ExportFlags.GetType()))}]");
			PrintConfigName(Environment.NewLine, padRightDistance: 37);
			PrintValue(options.ExportFlags.ToFlagsString());

			PrintConfigLine(Res.ConfigCurrentFileSaveSlot, $"{CmdOptions.CurrentSaveSlot} [1-4|0={Res.All}]", options.CurrentFileSaveSlot == 0 ? Res.All : options.CurrentFileSaveSlot.ToString());

			PrintConfigLine(Res.ConfigComparisonFileSaveSlot, $"{CmdOptions.ComparisonSaveSlot} [1-4|0={Res.All}]", options.ComparisonFileSaveSlot == 0 ? Res.CompSameAsCurrentFileSaveSlot : options.ComparisonFileSaveSlot.ToString());

			PrintConfigLine(Res.ConfigColorizeOutput, $"{CmdOptions.ColorizeOutput} [true|1|false|0]", options.ColorizeOutput.ToString());
			PrintConfigLine(Res.ConfigUILanguage, CmdOptions.UILanguage, options.UILanguage!);
			PrintConfigLine(Res.ConfigComparisonResultLanguage, CmdOptions.ComparisonResultLanguage, options.ComparisonResultLanguage!);
			PrintConfigLine(Res.ConfigFilePath, CmdOptions.ConfigPath, options.ConfigPath!);
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
			SetForegroundColor(ConsoleColor.DarkYellow);

			var startMessage = @$"== {Res.StartMessage.InsertArgs(nameof(Commands.Help), nameof(Commands.Guide_Srm))} ==";
			var length = Math.Min(startMessage.Length, Console.WindowWidth - 1);

			PrintLine("=".Repeat(length));
			PrintLine(startMessage);
			PrintLine("=".Repeat(length));

			PrintLine();
			ResetColor();
		}

		public virtual void PrintCommands()
		{
			PrintSectionHeader();

			PrintGroupName(Res.CmdGroupComparison);

			PrintCommand(Commands.Compare);
			PrintCommandKey(Commands.OverwriteComp);

			PrintGroupName(Res.CmdGroupSetsSaveSlot);

			PrintCommand(Commands.SetSlot);
			PrintCommand(Commands.SetSlot_Comp);
	
			PrintGroupName(Res.CmdGroupBackup);

			PrintCommand(Commands.Backup);
			PrintCommand(Commands.Backup_Comp);
			PrintCommand(Commands.Restore);
			PrintCommand(Commands.Restore_Comp);
			PrintCommand(Commands.Export);
			PrintCommand(Commands.ExportFlags);
			PrintCommand(Commands.Transfer);

			PrintGroupName(Res.CmdGroupDisplay);

			PrintCommand(Commands.Guide_Srm);
			PrintCommand(Commands.Guide_Savestate);
			PrintCommand(Commands.Help);
			PrintCommand(Commands.Config);
			PrintCommand(Commands.Clear);

			PrintGroupName(Res.CmdGroupMisc);

			PrintCommand(Commands.ChecksumStatus);
			PrintCommand(Commands.SlotByteComp);
			PrintCommand(Commands.NonSlotComp);
			PrintCommand(Commands.Offset);
			PrintCommand(Commands.EditOffset);
	
			PrintGroupName(Res.CmdGroupLanguage);

			PrintCommand(Commands.Lang);
			PrintCommand(Commands.Lang_Comp);

			PrintGroupName(Res.CmdGroupConfig);

			PrintCommand(Commands.LoadConfig);
			PrintCommand(Commands.SaveConfig);
			PrintCommand(Commands.OpenConfig);
			PrintCommand(Commands.AutoLoadOn);
			PrintCommand(Commands.AutoLoadOff);
			PrintCommand(Commands.CreateBindings);
			PrintCommand(Commands.OpenBindings);

			PrintCustomCommands();

			PrintLine();
			PrintLine();
			PrintCommand(Commands.Quit);
			PrintLine();

			PrintColoredLine(ConsoleColor.Cyan, Res.EnterCommand);
			ResetColor();

			void PrintCommand(Commands command)
			{
				PrintCommandKey(command);
				PrintColoredLine(ConsoleColor.Yellow, command.GetDisplayName()!);
			}
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

		public virtual void PrintFlags(in Enum flags, string? name = null)
		{
			if(name is not null)
				this.PrintSectionHeader(name);
			
			PrintColored(ConsoleColor.Yellow, $"{name ?? flags.GetType().GetDisplayName()}: ");
			PrintColoredLine(ConsoleColor.Cyan, $"[{flags.GetFlags().Join()}]");
			PrintColoredLine(ConsoleColor.Yellow, @" " + flags.GetSetFlags().Join());
	
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
			var sign = GetNumberSign((int)(currValue - compValue));
			int change = (int)currValue - (int)compValue;
			var absChange = (uint)Math.Abs(change);
			var changeString = $"{absChange,5:###}";
			var isNegativechange = change < 0;

			var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
			var compText = $"{compValue,3:D9} [x{compValue,2:X8}] [{compValue.FormatBinary(32)}]";
			var currText = $"{currValue,3:D9} [x{currValue,2:X8}] [{currValue.FormatBinary(32)}]";
			var changeText = $"{changeString,5} [x{absChange,2:X8}] [{absChange.FormatBinary(32)}]";

			PrintComparisonIdentification(ident);
			PrintOffsetValues(offsetText, offsetName);
			PrintCompValues(isNegativechange, compText);
			PrintLine(ident);
			PrintCurrValues(isNegativechange, currText);
			PrintLine(ident);
			PrintChangeValues(isNegativechange, absChange, sign, changeText);
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, ushort currValue,
			ushort compValue)
		{
			var sign = GetNumberSign(currValue - compValue);
			int change = currValue - compValue;
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

			var signColor = isNegativeChange ? ConsoleColor.DarkRed : ConsoleColor.Green;
			var changeColor = isNegativeChange ? ConsoleColor.Red : ConsoleColor.DarkGreen;
			
			var changedBits = changeValue.CountChangedBits();
			var oneBitColor = ConsoleColor.Yellow;

			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompChangeShort} ");

			if (changedBits == 1)
				signColor = changeColor = isNegativeChange ? ConsoleColor.Magenta : ConsoleColor.Green;

			PrintColored(signColor, sign);
			PrintColored(changeColor, changeText);
			PrintColored(ConsoleColor.DarkGray, ConsoleColor.Black, ":");

			PrintColored(changedBits switch
			{
				1 => ConsoleColor.Magenta,
				_ => ConsoleColor.DarkGray
			}, changedBits.ToString());

			if (changedBits == 1)
				PrintColored(oneBitColor, "!");

			PrintLine();

			ResetColor();
		}

		protected virtual string GetNumberSign(int value) => Math.Sign(value) < 0 ? "(-)" : "(+)";

		protected virtual void PrintComparisonIdentification(string ident) => PrintColored(ConsoleColor.White, $@"{ident}=> ");

		protected virtual void PrintColored(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text)
		{
			SetBackgroundColor(backgroundColor);
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
		public virtual string NewLine => NewLineDefault;

		public virtual void PrintLine() => PrintLine(string.Empty);
		public virtual void PrintLine(string text) => Print(text + NewLine + Prefix);
		public virtual void Print(string text) => Console.Write(text);
		
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

		public virtual void SetForegroundColor(ConsoleColor color)
		{
			if (!ColorizeOutput) return;
			
			Console.ForegroundColor = color;
		}

		public virtual void SetBackgroundColor(ConsoleColor color)
		{
			if (!ColorizeOutput) return;
			
			Console.BackgroundColor = color;
		}
	}
}