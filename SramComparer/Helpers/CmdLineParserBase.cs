using System;
using System.Collections.Generic;
using System.IO;
using App.Commons.Extensions;
using SramComparer.Properties;

namespace SramComparer.Helpers
{
    public abstract class CmdLineParserBase<TOptions, TFileRegion, TGameId, TComparisonFlags>
        where TOptions : OptionsBase<TFileRegion, TGameId, TComparisonFlags>, new()
        where TFileRegion : struct, Enum
        where TGameId : struct, Enum
        where TComparisonFlags : struct, Enum
    {
        public static TOptions Parse(IReadOnlyList<string> args)
        {
            if (args.Count == 0) return new TOptions();

            int i;
            var currentGameFile = args[0];
            var argsLength = args.Count;
            var options = new TOptions { CurrentGameFilepath = currentGameFile };

            if (options.CurrentGameFilepath is not null)
            {
                if (Path.GetExtension(currentGameFile).ToLower() != ".srm")
                    throw new ArgumentException(Resources.ErrorGameFileIsNotSrmTemplate.InsertArgs(Resources.Comparison), nameof(options.CurrentGameFilepath));

                options.ComparisonGameFilepath = Path.Join(Path.GetDirectoryName(currentGameFile),
                    Path.GetFileNameWithoutExtension(currentGameFile) + $" ### {Resources.Comparison}" +
                    Path.GetExtension(currentGameFile));
                options.ExportDirectory = Path.GetDirectoryName((string?) options.ComparisonGameFilepath);
            }

            for (i = 1; i < argsLength; i += 2)
            {
                var cmdName = args[i].ToLower();
                var value = args[i + 1];

                switch (cmdName)
                {
                    case CmdOptions.ComparisonFile:
                        options.ComparisonGameFilepath = value;
                        break;
                    case CmdOptions.Exportdir:
                        options.ExportDirectory = value;
                        break;
                    case CmdOptions.Game:
                        options.Game = value.ParseEnum<TGameId>();
                        break;
                    case CmdOptions.ComparisonGame:
                        options.ComparisonGame = value.ParseEnum<TGameId>();
                        break;
                    case CmdOptions.Region:
                        options.Region = value.ParseEnum<TFileRegion>();
                        break;
                    case CmdOptions.ComparisonFlags:
                        options.Flags = value.ParseEnum<TComparisonFlags>();
                        break;
                }
            }

            if (Path.GetExtension(options.ComparisonGameFilepath).ToLower() != ".srm")
                throw new ArgumentException(Resources.ErrorGameFileIsNotSrmTemplate.InsertArgs(Resources.Comparison), nameof(options.ComparisonGameFilepath));

            return options;
        }
    }
}