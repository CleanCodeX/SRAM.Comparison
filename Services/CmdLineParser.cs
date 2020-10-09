using System;
using System.Collections.Generic;
using System.IO;
using Common.Shared.Min.Extensions;
using SramComparer.Extensions;
using SramComparer.Properties;

namespace SramComparer.Services
{
	/// <summary>
	/// Standard implementation of <see cref="ICmdLineParser"/>
	/// </summary>
	/// <typeparam name="TOptions">The options's type t be parsed into</typeparam>
	/// <typeparam name="TFileRegion">The game file's region enum</typeparam>
	/// <typeparam name="TComparisonFlags">The game's comparison flags enum</typeparam>
	public class CmdLineParser<TOptions, TFileRegion, TComparisonFlags> : ICmdLineParser
		where TOptions : Options<TFileRegion, TComparisonFlags>, new()
		where TFileRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{
		/// <summary>
		/// Parses a list of string arguments into an <see cref="IOptions"/> instance
		/// </summary>
		/// <param name="args">The string arguments to be parsed</param>
		/// <returns>Returns an <see cref="IOptions"/> instance</returns>
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
				throw new ArgumentException(Resources.ErrorGameFileIsNotSrmFileTypeFilepathTemplate.InsertArgs(Resources.Current, options.CurrentGameFilepath), nameof(options.CurrentGameFilepath));

			options.ExportDirectory = Path.GetDirectoryName(options.CurrentGameFilepath);
			options.ComparisonGameFilepath = currentGameFile + compFileExtension;
			
			int i;
			for (i = 1; i < args.Count; i += 2)
			{
				var cmdName = args[i].ToLower();

				if (i == args.Count - 1)
					throw new ArgumentException(Resources.ErrorParamNameValueMissmatchTemplate.InsertArgs(cmdName));

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