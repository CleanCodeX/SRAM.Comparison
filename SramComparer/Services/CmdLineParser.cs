using System;
using System.Collections.Generic;
using System.IO;
using App.Commons.Extensions;
using SramComparer.Extensions;
using SramComparer.Properties;

namespace SramComparer.Services
{
    public class CmdLineParser<TOptions, TFileRegion, TComparisonFlags> : ICmdLineParser
        where TOptions : Options<TFileRegion, TComparisonFlags>, new()
        where TFileRegion : struct, Enum
        where TComparisonFlags : struct, Enum
    {
        public virtual IOptions Parse(IReadOnlyList<string> args)
        {
            if (args.Count == 0) return new TOptions();

            const string srmFileExtension = ".srm";
            const string compFileExtension = ".comp";
            var currentGameFile = args[0];
            var options = new TOptions { CurrentGameFilepath = currentGameFile };

            if (currentGameFile.IsNullOrEmpty())
                throw new ArgumentException(Resources.ErrorMissingPathArguments, nameof(options.CurrentGameFilepath));
            
            if (Path.GetExtension(currentGameFile).ToLower() != srmFileExtension)
                throw new ArgumentException(Resources.ErrorGameFileIsNotSrmFileTypeFilepathTemplate.InsertArgs(Resources.Comparison, options.CurrentGameFilepath), nameof(options.CurrentGameFilepath));

            options.ExportDirectory = Path.GetDirectoryName(options.CurrentGameFilepath);

            options.ComparisonGameFilepath = Path.Join(Path.GetDirectoryName(currentGameFile),
                Path.GetFileNameWithoutExtension(currentGameFile) + compFileExtension);
            
            int i;
            for (i = 1; i < args.Count; i += 2)
            {
                var cmdName = args[i].ToLower();
                var value = args[i + 1];

                switch (cmdName)
                {
                    case CmdOptions.Command:
                        options.Commands = value.IsNullOrEmpty() ? null : value;
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

            if (options.ComparisonGameFilepath.IsNullOrEmpty())
                throw new ArgumentException(Resources.ErrorMissingPathArguments, nameof(options.ComparisonGameFilepath));

            if (Path.GetExtension(options.ComparisonGameFilepath).ToLower() != compFileExtension)
                throw new ArgumentException(Resources.ErrorGameFileIsNotSrmFileTypeFilepathTemplate.InsertArgs(Resources.Comparison, options.ComparisonGameFilepath), nameof(options.ComparisonGameFilepath));

            return options;
        }
    }
}