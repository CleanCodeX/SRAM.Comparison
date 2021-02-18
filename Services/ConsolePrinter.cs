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
		private static readonly string NewLineDefault = Environment.NewLine;
		private const string Nbsp = "\u00A0";

		protected string LinePrefix = Nbsp;
		protected string BufferInfoValueSeparator = "|"; // ¦
		protected string CandidateMarker = "[!]";
		protected string ComparisonMarker = "¬"; // ¬
		protected string CompValueMarker = ":";
		protected string BufferNameMarker = "~";
		protected string CurrValueMarker = "»";
		protected string ChangeMarker = "»";
		protected string PositiveChangeMarker = "(+)";
		protected string NegativeChangeMarker = "(-)";
		protected string HeaderTextTemplate = @"===[{0}]====================================================";
		protected string CmdLineParamExampleValuesTemplate = "[{0}]";
		protected string CmdAltKeysTemplate = "[{0}]";
		protected string EnumValuesTemplate = "[{0}]";
		protected string EnumValuesSeparator = "|";
		protected int CommandNameColumnLength = 30;
		protected string WramOffsetTemplate = "$7E:{0}";

		public virtual void PrintConfig(IOptions options)
		{
			PrintSectionHeader();
			PrintColoredLine(ConsoleColor.Gray, Res.Config + @":");

			PrintConfigLine(Res.CurrentFilePath, "{0}", Path.GetFileName(options.CurrentFilePath!));
			PrintConfigLine(Res.ComparisonPath, "{1-2}|" + CmdOptions.ComparisonPath, Path.GetFileName(FilePathHelper.GetComparisonFilePath(options)));

			PrintConfigLine(Res.EnumGameRegion, "{1-2}|" + CmdOptions.GameRegion, GetEnumNames(options.GameRegion.GetType()), options.GameRegion.ToString());

			var newLinePadRightLength = 38;
			PrintConfigName(Res.EnumComparisonFlags, CmdOptions.ComparisonFlags, GetEnumNames(options.ComparisonFlags.GetType()));
			PrintConfigName(Environment.NewLine, padRightDistance: newLinePadRightLength);
			PrintValueLineBreak(options.ComparisonFlags.ToFlagsString());

			#region Export

			PrintConfigLine(Res.ExportPath, CmdOptions.ExportPath, options.ExportPath ?? Path.GetDirectoryName(options.CurrentFilePath)!);

			PrintConfigName(Res.EnumExportFlags, CmdOptions.ExportFlags, GetEnumNames(options.ExportFlags.GetType()));
			PrintConfigName(Environment.NewLine, padRightDistance: newLinePadRightLength);
			PrintValueLineBreak(options.ExportFlags.ToFlagsString());

			#endregion

			#region Save slot

			PrintConfigLine(Res.CurrentFileSaveSlot, CmdOptions.CurrentSaveSlot, $"1-4|0={Res.All}", options.CurrentFileSaveSlot == 0 ? Res.All : options.CurrentFileSaveSlot.ToString());
			PrintConfigLine(Res.ComparisonFileSaveSlot, CmdOptions.ComparisonSaveSlot, $"1-4|0={Res.All}", options.ComparisonFileSaveSlot == 0 ? Res.CompSameAsCurrentFileSaveSlot : options.ComparisonFileSaveSlot.ToString());

			#endregion

			#region Language

			PrintConfigLine(Res.UILanguage, CmdOptions.UILanguage, options.UILanguage!);
			PrintConfigLine(Res.ComparisonResultLanguage, CmdOptions.ComparisonResultLanguage, options.ComparisonResultLanguage!);

			#endregion

			PrintConfigLine(Res.ColorizeOutput, CmdOptions.ColorizeOutput, "true|1|false|0", options.ColorizeOutput.ToString());
			PrintConfigLine(Res.ConfigPath, CmdOptions.ConfigPath, options.ConfigPath!);
			PrintConfigLine(Res.CmdWatchCurrentFile, CmdOptions.AutoWatch, "true|1|false|0", options.AutoWatch.ToString());
			PrintConfigLine(Res.AutoSave, CmdOptions.AutoSave, "true|1|false|0", options.AutoSave.ToString());

			#region Logging

			PrintConfigName(Res.EnumLogFlags, CmdOptions.ComparisonFlags, GetEnumNames(options.LogFlags.GetType()));
			PrintConfigName(Environment.NewLine, padRightDistance: newLinePadRightLength);
			PrintValueLineBreak(options.LogFlags.ToFlagsString());

			PrintConfigLine(Res.LogPath, CmdOptions.LogPath, options.LogPath!);

			#endregion

			#region FileWatchFlags

			PrintConfigName(Res.EnumFileWatchFlags, CmdOptions.WatchFlags, GetEnumNames(options.FileWatchFlags.GetType()));
			PrintConfigName(Environment.NewLine, padRightDistance: newLinePadRightLength);
			PrintValueLineBreak(options.FileWatchFlags.ToFlagsString());

			#endregion
		}

		private string GetEnumNames(Type enumType) => string.Join(EnumValuesSeparator, Enum.GetNames(enumType));

		public void PrintConfigLine(string name, string value) => PrintConfigLine(name, null!, value);
		public void PrintConfigLine(string name, string args, string value)
		{
			PrintConfigName(name, args);
			PrintValueLineBreak(value);
		}

		public void PrintConfigLine(string name, string argName, string examples, string value)
		{
			PrintConfigName(name, argName);
			PrintColored(ConsoleColor.DarkCyan, Nbsp + CmdLineParamExampleValuesTemplate.InsertArgs(examples));
			PrintValueLineBreak(value);
		}

		public void Clear() => Console.Clear();

		protected virtual void PrintCustomCommands() {}

		public virtual void PrintStartMessage()
		{
			PrintSectionHeader();
			SetForegroundColor(ConsoleColor.DarkYellow);

			var startMessage = @$"== {Res.StartMessageTemplate.InsertArgs(nameof(Commands.Help), nameof(Commands.SrmGuide))} ==";
			var length = Math.Min(startMessage.Length, Console.WindowWidth - 1);

			PrintLine("=".Repeat(length));
			PrintLine(startMessage);
			PrintLine("=".Repeat(length));

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
			PrintCommand(Commands.SetCompSlot);
			PrintCommand(Commands.ShowSlotSummary);
			
			PrintGroupName(Res.CmdGroupBackup);

			PrintCommand(Commands.Backup);
			PrintCommand(Commands.BackupComp);
			PrintCommand(Commands.Restore);
			PrintCommand(Commands.RestoreComp);
			PrintCommand(Commands.ExportComparison);
			PrintCommand(Commands.ExportSlotSummary);
			PrintCommand(Commands.Transfer);
			
			PrintGroupName(Res.CmdGroupDisplay);

			PrintCommand(Commands.SrmGuide);
			PrintCommand(Commands.SavestateGuide);
			PrintCommand(Commands.Help);
			PrintCommand(Commands.Config);
			PrintCommand(Commands.Clear);

			PrintGroupName(Res.CmdGroupMisc);

			PrintCommand(Commands.Offset);
			PrintCommand(Commands.EditOffset);
			PrintCommand(Commands.ChecksumStatus);

			PrintGroupName(Res.CmdGroupLanguage);

			PrintCommand(Commands.Lang);
			PrintCommand(Commands.CompLang);

			PrintGroupName(Res.CmdGroupConfig);

			PrintCommand(Commands.WatchFile);
			PrintCommand(Commands.UnwatchFile);
			PrintCommand(Commands.SlotByteComp);
			PrintCommand(Commands.NonSlotComp);
			PrintCommand(Commands.ExportFlags);
			PrintCommand(Commands.FileWatchFlags);
			PrintCommand(Commands.LogFlags);
			PrintCommand(Commands.ComparisonFlags);
			PrintCommand(Commands.LoadConfig);
			PrintCommand(Commands.SaveConfig);
			PrintCommand(Commands.OpenConfig);
			PrintCommand(Commands.OpenLog);
			PrintCommand(Commands.AutoLoadOn);
			PrintCommand(Commands.AutoLoadOff);
			PrintCommand(Commands.AutoSaveOn);
			PrintCommand(Commands.AutoSaveOff);
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

		protected virtual void PrintCommandKey(Enum key)
		{
			PrintColored(ConsoleColor.White, key.ToString());
			PrintColored(ConsoleColor.DarkCyan, $"{GetAlternateCommands(key).PadRight(CommandNameColumnLength - key.ToString().Length)}");
			PrintColored(ConsoleColor.White, ": ");
		}

		protected virtual string GetAlternateCommands(Enum cmd)
		{
			var compValue = cmd.ToInt();
			var dict = default(AlternateCommands).ToDictionary();
			var altKeys = dict
				.Where(e => e.Value.ToInt() == compValue)
				.Select(e => e.Key).ToArray();

			return altKeys.Length > 0 ? Nbsp + CmdAltKeysTemplate.InsertArgs(altKeys.Join()) : string.Empty;
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
			PrintColoredLine(ConsoleColor.Yellow, Nbsp + flags.HasUInt32Flag(flag));
			ResetColor();
		}

		public virtual void PrintFlags(in Enum flags, string? name = null)
		{
			if(name is not null)
				this.PrintSectionHeader(name);
			
			PrintColored(ConsoleColor.Yellow, $"{name ?? flags.GetType().GetDisplayName()}: ");
			PrintColoredLine(ConsoleColor.DarkCyan, EnumValuesTemplate.InsertArgs(flags.GetFlags().Join()) + Nbsp);
			PrintColored(ConsoleColor.Yellow, $"{Res.FlagsSet} ");
			PrintColoredLine(ConsoleColor.Cyan, flags.ToFlagsString());
			ResetColor();
		}

		protected virtual void PrintCustomBufferInfo() {}
		public virtual void PrintBufferInfo(string name, int offset, int size, int? wramOffset = null)
		{
			PrintLine();

			PrintColored(ConsoleColor.White, Nbsp.Repeat(4) + @$"[ {Res.CompSection} ");

			PrintColored(ConsoleColor.DarkYellow, name);
			PrintColored(ConsoleColor.White, $@" {BufferInfoValueSeparator} ");
			PrintColored(ConsoleColor.White, $@"{Res.CompOffset} ");
			PrintColored(ConsoleColor.DarkYellow, offset + $@" [x{offset:X}]");

			if(wramOffset is not null)
				PrintColored(ConsoleColor.DarkYellow, $@" [{WramOffsetTemplate.InsertArgs($"{wramOffset:X4}")}]");

			PrintColored(ConsoleColor.White, $@" {BufferInfoValueSeparator} ");

			PrintColored(ConsoleColor.White, $@"{Res.CompSize} ");
			PrintColored(ConsoleColor.DarkYellow, size.ToString());

			PrintColored(ConsoleColor.White, @" ]");

			PrintCustomBufferInfo();

			PrintLine();
			ResetColor();
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, uint currValue, uint compValue, bool isUnknown)
		{
			var sign = GetNumberSign((int)(currValue - compValue));
			int change = (int)currValue - (int)compValue;
			var absChange = (uint)Math.Abs(change);
			var isNegativechange = change < 0;

			var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
			var compText = $"{compValue,3:D9} [x{compValue,2:X8}] [{compValue.FormatBinary(32)}]";
			var currText = $"{currValue,3:D9} [x{currValue,2:X8}] [{currValue.FormatBinary(32)}]";
			var changeText = $"{absChange} [x{absChange:X8}] [{absChange.FormatBinary(32)}]";

			PrintComparisonIdentification(ident);
			PrintOffsetValues(offsetText, offsetName);
			
			var newLineIdent = ident + Nbsp.Repeat(2);
			PrintLine();
			Print(newLineIdent);
			PrintCompValues(isNegativechange, compText);
			PrintLine();
			Print(newLineIdent);
			PrintCurrValues(isNegativechange, currText);
			PrintLine();
			Print(newLineIdent);
			PrintChangeValues(isNegativechange, absChange, sign, changeText, isUnknown);
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, ushort currValue, ushort compValue, bool isUnknown)
		{
			var sign = GetNumberSign(currValue - compValue);
			int change = currValue - compValue;
			var absChange = (uint)Math.Abs(change);
			var isNegativechange = change < 0;

			var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
			var compText = $"{compValue,3:D5} [x{compValue,2:X4}] [{compValue.FormatBinary(16)}]";
			var currText = $"{currValue,3:D5} [x{currValue,2:X4}] [{currValue.FormatBinary(16)}]";
			var changeText = $"{absChange} [x{absChange:X4}] [{absChange.FormatBinary(16)}]";

			PrintComparisonIdentification(ident);
			PrintOffsetValues(offsetText, offsetName);
			PrintCompValues(isNegativechange, compText);
			PrintCurrValues(isNegativechange, currText);
			var newLineIdent = ident + Nbsp.Repeat(2);
			PrintLine();
			Print(newLineIdent);
			PrintChangeValues(isNegativechange, absChange, sign, changeText, isUnknown);
		}

		public virtual void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue, bool isUnknown)
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
			var newLineIdent = ident + Nbsp.Repeat(2);
			PrintLine();
			Print(newLineIdent);
			PrintChangeValues(isNegativechange, absChange, sign, changeText, isUnknown);
		}

		protected virtual string GetByteValueRepresentations(byte value) => NumberFormatter.GetByteValueRepresentations(value);

		protected virtual string GetByteValueChangeRepresentations(byte currValue, byte compValue) => NumberFormatter.GetByteValueChangeRepresentations(currValue, compValue);

		public virtual void PrintSectionHeader()
		{
			ResetColor();
			PrintLine();
			PrintColoredLine(ConsoleColor.Cyan, HeaderTextTemplate.InsertArgs(DateTime.Now.ToLongTimeString()));
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

		protected virtual void PrintConfigName(string settingName, string cmdArgName, string examples, int padRightDistance = 35)
		{
			PrintColored(ConsoleColor.White, settingName.PadRight(padRightDistance) + @":");
			PrintColored(ConsoleColor.Cyan, cmdArgName);
			PrintColored(ConsoleColor.DarkCyan, Nbsp + CmdLineParamExampleValuesTemplate.InsertArgs(examples));
		}

		protected virtual void PrintConfigName(string settingName, string? cmdArg = null, int padRightDistance = 35)
		{
			PrintColored(ConsoleColor.White, settingName.PadRight(padRightDistance) + @":");

			if (cmdArg is null) return;

			PrintColored(ConsoleColor.Cyan, cmdArg);
		}

		protected virtual void PrintValue(object value) => PrintColored(ConsoleColor.Yellow, Nbsp + value);
		protected virtual void PrintValueLineBreak(object value) => PrintColoredLine(ConsoleColor.Yellow, Nbsp + value);

		protected virtual void PrintOffsetValues(string offsetText, string? offsetName)
		{
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompOffset} ");
			PrintColored(ConsoleColor.DarkCyan, offsetText);

			if (offsetName is null) return;

			PrintColored(ConsoleColor.Cyan, $@" {BufferNameMarker} ");
			PrintColored(ConsoleColor.Yellow, offsetName);
		}

		protected virtual void PrintCompValues(bool isNegativeChange, string compText)
		{
			PrintColored(ConsoleColor.Cyan, $@" {CompValueMarker} ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompOldValue} ");
			PrintColored(isNegativeChange ? ConsoleColor.Green : ConsoleColor.Red, compText);
		}

		protected virtual void PrintCurrValues(bool isNegativeChange, string currText)
		{
			PrintColored(ConsoleColor.Cyan, $@" {CurrValueMarker} ");
			PrintColored(ConsoleColor.DarkGray, $@"{Res.CompNewValue} ");
			PrintColored(isNegativeChange ? ConsoleColor.Red : ConsoleColor.Green, currText);
		}

		protected virtual void PrintChangeValues(bool isNegativeChange, uint changeValue, string sign, string changeText, bool isUnknown)
		{
			var signColor = isNegativeChange ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
			var changeColor = isNegativeChange ? ConsoleColor.Red : ConsoleColor.Green;
			
			var changedBits = changeValue.CountChangedBits();
			var oneBitColor = ConsoleColor.Yellow;
			var highlightBgColor = ConsoleColor.DarkBlue;
			var bitsColor = ConsoleColor.DarkGray;

			if (isUnknown && changedBits == 1)
			{
				highlightBgColor = ConsoleColor.Blue;
				bitsColor = ConsoleColor.White;
				signColor = changeColor = isNegativeChange ? ConsoleColor.Magenta : ConsoleColor.Green;
			}

			PrintColored(bitsColor, highlightBgColor, Res.CompChangeShort);
			SetBackgroundColor(ConsoleColor.Black);
			
			PrintColored(signColor, Nbsp + sign);
			PrintColored(changeColor, Nbsp + changeText);
			PrintColored(ConsoleColor.Cyan, ConsoleColor.Black, Nbsp + ChangeMarker + Nbsp);

			PrintColored(bitsColor, highlightBgColor, changedBits.ToString());

			if (changedBits == 1)
			{
				PrintColored(bitsColor, Nbsp + Res.Bit);
				if (isUnknown)
				{
					PrintColored(oneBitColor, ConsoleColor.Black, Nbsp + CandidateMarker + Nbsp);
					PrintColored(bitsColor, ConsoleColor.DarkBlue, "»" + Nbsp);
					PrintColored(bitsColor, Res.SingleBitChangeText);
					PrintColored(bitsColor, Nbsp + "«");
				}
			}
			else
				PrintColored(bitsColor, Nbsp + Res.Bits);

			SetBackgroundColor(ConsoleColor.Black);
			PrintLine();
			
			ResetColor();
		}

		protected virtual string GetNumberSign(int value) => Math.Sign(value) < 0 ? NegativeChangeMarker : PositiveChangeMarker;
		
		protected virtual void PrintComparisonIdentification(string ident) => PrintColored(ConsoleColor.Cyan, $@"{ident}{ComparisonMarker} ");

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
		public virtual void PrintLine(string text) => Print(text + NewLine + LinePrefix);
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
			PrintColored(foregroundColor, text);
			SetBackgroundColor(ConsoleColor.Black);
			Print(NewLine);
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