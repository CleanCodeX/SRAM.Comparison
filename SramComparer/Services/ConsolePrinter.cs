using App.Commons.Extensions;
using SramCommons.Extensions;
using SramComparer.Enums;
using SramComparer.Extensions;
using System;
using System.IO;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Services
{
    public class ConsolePrinter: IConsolePrinter
    {
        public virtual void PrintSettings(IOptions options)
        {
            PrintSectionHeader();
            Console.WriteLine(Res.Settings + @":");

            PrintSettingName(Res.SettingCurrentGameFilepath, "{0}");
            PrintValue(Path.GetFileName(options.CurrentGameFilepath));

            PrintSettingName(Res.SettingComparisonGameFilepath, CmdOptions.ComparisonFile);
            PrintValue(Path.GetFileName(options.ComparisonGameFilepath));

            PrintSettingName(Res.SettingExportDirectory, CmdOptions.Exportdir);
            PrintValue(options.ExportDirectory);

            PrintSettingName(Res.SettingCurrentGameToCompare, $"{CmdOptions.Game} [1-4|0={Res.All}]");
            PrintValue(options.Game == 0 ? Res.All : options.Game.ToString());

            PrintSettingName(Res.SettingComparisonGameToCompare, $"{CmdOptions.ComparisonGame} [1-4|0={Res.All}]");
            PrintValue(options.ComparisonGame == 0 ? Res.SameAsCurrentGame : options.ComparisonGame.ToString());

            PrintSettingName(Res.SettingRegion, $"{CmdOptions.Region} [{string.Join("|", Enum.GetNames(options.Region.GetType()))}]");
            PrintValue(options.Region.ToString());

            PrintSettingName(Res.ComparisonFlags, $"{CmdOptions.ComparisonFlags} [{string.Join(",", Enum.GetNames(options.Flags.GetType()))}]");
            PrintValue(Environment.NewLine.PadRight(30) + options.Flags.ToFlagsString());
        }

        protected virtual void PrintCustomCommands() {}

        public virtual void PrintStartMessage()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.WriteLine("=".Repeat(50));
            Console.WriteLine(@"== " + Res.StartMessage.InsertArgs(nameof(BaseCommands.cmd), nameof(BaseCommands.m)));
            Console.WriteLine("=".Repeat(50));
        }

        public virtual void PrintCommands()
        {
            PrintSectionHeader();

            PrintGroupName(Res.CmdGroupComparison);

            PrintCommandKey(BaseCommands.c);
            Console.WriteLine(Res.CommandCompareFiles);

            PrintCommandKey(BaseCommands.ow);
            Console.WriteLine(Res.CommandOverwriteComparisonFile);

            PrintGroupName(Res.CmdGroupSetGame);

            PrintCommandKey(BaseCommands.sg);
            Console.WriteLine(Res.CommandSetGame);

            PrintCommandKey(BaseCommands.sgc);
            Console.WriteLine(Res.CommandSetComparisonGame);

            PrintGroupName(Res.CmdGroupBackup);

            PrintCommandKey(BaseCommands.b);
            Console.WriteLine(Res.CommandBackupCurrentFile);

            PrintCommandKey(BaseCommands.bc);
            Console.WriteLine(Res.CommandBackupComparisonFile);

            PrintCommandKey(BaseCommands.r);
            Console.WriteLine(Res.CommandRestoreCurrentFile);

            PrintCommandKey(BaseCommands.rc);
            Console.WriteLine(Res.CommandRestoreComparisonFile);

            PrintCommandKey(BaseCommands.e);
            Console.WriteLine(Res.CommandExportComparisonResult);

            PrintCommandKey(BaseCommands.ts);
            Console.WriteLine(Res.CommandTransferSramToSimilarGameFile);

            PrintGroupName(Res.CmdGroupDisplay);

            PrintCommandKey(BaseCommands.m);
            Console.WriteLine(Res.CommandManual);

            PrintCommandKey(BaseCommands.cmd);
            Console.WriteLine(Res.CommandDisplayCommands);

            PrintCommandKey(BaseCommands.s);
            Console.WriteLine(Res.CommandDisplaySettings);

            PrintCommandKey(BaseCommands.w);
            Console.WriteLine(Res.CommandWipeOutput);

            PrintGroupName(Res.CmdOther);

            PrintCommandKey(BaseCommands.fwg);
            Console.WriteLine(Res.CommandIncludeWholeGameBufferComparison);

            PrintCommandKey(BaseCommands.fng);
            Console.WriteLine(Res.CommandIncludeNonGameBufferComparison);

            PrintCustomCommands();

            Console.WriteLine();
            Console.WriteLine();
            PrintCommandKey(BaseCommands.q);
            Console.WriteLine(Res.CommandQuit);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Res.EnterCommand);
            Console.ResetColor();
        }

        protected virtual void PrintGroupName(string groupName)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(Environment.NewLine + groupName);
        }

        protected virtual void PrintCommandKey(Enum key)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@$"{key,12}: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        protected virtual string GetManualText() => Res.AppManualCommandsTemplate.InsertArgs(
            BaseCommands.ow, BaseCommands.c, BaseCommands.e, BaseCommands.sg, BaseCommands.sgc, BaseCommands.fwg, BaseCommands.fng,
            BaseCommands.b, BaseCommands.bc, BaseCommands.r, BaseCommands.rc);

        protected virtual string GetAppDescriptionText() => Res.AppDescription;

        public virtual void PrintManual()
        {
            PrintSectionHeader();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(GetAppDescriptionText());
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine(GetManualText());
            Console.ResetColor();
        }

        public virtual void PrintInvertIncludeFlag(Enum flags, Enum flag)
        {
            PrintSectionHeader();
            Console.Write(flag + @":");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" " + flags.HasFlag(flag));
            Console.ResetColor();
        }

        protected virtual void PrintCustomBufferInfo() {}
        public virtual void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength)
        {
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" ".Repeat(4) + @$"[ {Res.Section} ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(bufferName);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" | ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($@"{Res.Offset} ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(bufferOffset + $@" [x{bufferOffset:X}]");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" | ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($@"{Res.Size} ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(bufferLength);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" ]");

            PrintCustomBufferInfo();

            Console.WriteLine();
            Console.ResetColor();
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
            var changeString = $"{absChange,3:###}";
            var isNegativechange = change < 0;

            var offsetText = $"{offset,4:D4} [x{offset,3:X3}]";
            var compText = $"{compValue,3:D3} [x{compValue,2:X2}] [{compValue.FormatBinary(8)}]";
            var currText = $"{currValue,3:D3} [x{currValue,2:X2}] [{currValue.FormatBinary(8)}]";
            var changeText = $"{changeString,3} [x{absChange,2:X2}] [{absChange.FormatBinary(8)}]";

            PrintComparisonIdentification(ident);
            PrintOffsetValues(offsetText, offsetName);
            PrintCompValues(isNegativechange, compText);
            PrintCurrValues(isNegativechange, currText);
            PrintChangeValues(isNegativechange, absChange, sign, changeText);
        }

        public virtual void PrintSectionHeader()
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($@"===[ {DateTime.Now.ToLongTimeString()} ]====================================================");
            Console.ResetColor();
        }

        public virtual void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public virtual void PrintError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(ex);
            Console.ResetColor();
        }

        public virtual void PrintFatalError(string fataError)
        {
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(fataError);
            Console.ResetColor();

            Console.ReadKey();
        }

        protected virtual void PrintSettingName(string settingName, string? cmdArg = null)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(settingName.PadRight(28) + @":");

            if (cmdArg is null) return;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(cmdArg);
        }

        protected virtual void PrintValue(object value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" " + value);
        }

        protected virtual void PrintOffsetValues(string offsetText, string? offsetName)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($@"{Res.Offset} ");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(offsetText);

            if (offsetName is null) return;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(@" => ");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(offsetName);
        }

        protected virtual void PrintCompValues(bool isNegativeChange, string compText)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@" | ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($@"{Res.CompShort} ");
            Console.ForegroundColor = isNegativeChange ? ConsoleColor.DarkGreen : ConsoleColor.Red;
            Console.Write(compText);
        }

        protected virtual void PrintCurrValues(bool isNegativeChange, string currText)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(@" => ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($@"{Res.CurrShort} ");
            Console.ForegroundColor = isNegativeChange ? ConsoleColor.Red : ConsoleColor.DarkGreen;
            Console.Write(currText);
        }

        protected virtual void PrintChangeValues(bool isNegativeChange, uint changeValue, string sign, string changeText)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(@" = ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($@"{Res.ChangeShort} ");

            Console.ForegroundColor = isNegativeChange ? ConsoleColor.DarkRed : ConsoleColor.Green;
            Console.Write(sign);

            Console.ForegroundColor = isNegativeChange ? ConsoleColor.Red : ConsoleColor.DarkGreen;
            Console.Write(changeText);

            var changedBits = changeValue.CountChangedBits();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(":");

            Console.ForegroundColor = changedBits switch
            {
                1 => ConsoleColor.Magenta,
                8 => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray
            };
            Console.WriteLine(changedBits);

            Console.ResetColor();
        }

        protected virtual string GetNumberSign(short value) => Math.Sign(value) < 0 ? "(-)" : "(+)";

        protected virtual void PrintComparisonIdentification(string ident)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($@"{ident}=> ");
        }
    }
}