using System;
using System.Collections.Generic;
using System.IO;
using App.Commons.Extensions;
using SramComparer.Extensions;
using SramComparer.Properties;

namespace SramComparer.Helpers
{
    public abstract class CmdLineParserBase<TOptions, TFileRegion, TComparisonFlags>
        where TOptions : OptionsBase<TFileRegion, TComparisonFlags>, new()
        where TFileRegion : struct, Enum
        where TComparisonFlags : struct, Enum
    {
        public static TOptions Parse(IReadOnlyList<string> args)
        {
            if (args.Count == 0) return new TOptions();

            const string fileExtension = ".srm";
            var currentGameFile = args[0];
            var options = new TOptions { CurrentGameFilepath = currentGameFile };
            
            if (options.CurrentGameFilepath is not null)
            {
                if (Path.GetExtension(currentGameFile).ToLower() != fileExtension)
                    throw new ArgumentException(Resources.ErrorGameFileIsNotSrmFileTypeFilepathTemplate.InsertArgs(Resources.Comparison, options.CurrentGameFilepath), nameof(options.CurrentGameFilepath));

                var compFileNameSuffix = $" ### {Resources.Comparison}";
                options.ComparisonGameFilepath = Path.Join(Path.GetDirectoryName(currentGameFile),
                    Path.GetFileNameWithoutExtension(currentGameFile) + compFileNameSuffix +
                    Path.GetExtension(currentGameFile));
                options.ExportDirectory = Path.GetDirectoryName((string?) options.ComparisonGameFilepath);
            }

            int i;
            for (i = 1; i < args.Count; i += 2)
            {
                var cmdName = args[i].ToLower();
                var value = args[i + 1];

                switch (cmdName)
                {
                    case CmdOptions.Command:
                        options.Command = value;
                        break;
                    case CmdOptions.ComparisonFile:
                        options.ComparisonGameFilepath = value;
                        break;
                    case CmdOptions.Exportdir:
                        options.ExportDirectory = value;
                        break;
                    case CmdOptions.Game:
                        options.Game = value.ParseGameId();
                        break;
                    case CmdOptions.ComparisonGame:
                        options.ComparisonGame = value.ParseGameId();
                        break;
                    case CmdOptions.Region:
                        options.Region = value.ParseEnum<TFileRegion>();
                        break;
                    case CmdOptions.ComparisonFlags:
                        options.Flags = value.ParseEnum<TComparisonFlags>();
                        break;
                }
            }

            if (Path.GetExtension(options.ComparisonGameFilepath).ToLower() != fileExtension)
                throw new ArgumentException(Resources.ErrorGameFileIsNotSrmFileTypeFilepathTemplate.InsertArgs(Resources.Comparison, options.ComparisonGameFilepath), nameof(options.ComparisonGameFilepath));

            return options;
        }
    }
}