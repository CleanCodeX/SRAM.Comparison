#define Check_FileExtensions

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using SramComparer.Extensions;
using SramComparer.Properties;

namespace SramComparer.Services
{
	/// <summary>
	/// Standard implementation of <see cref="ICmdLineParser"/>
	/// </summary>
	/// <typeparam name="TOptions">The options's type t be parsed into</typeparam>
	/// <typeparam name="TRomRegion">The game file's region enum</typeparam>
	/// <typeparam name="TComparisonFlags">The game's comparison flags enum</typeparam>
	public class CmdLineParser<TOptions, TRomRegion, TComparisonFlags> : ICmdLineParser
		where TOptions : Options<TRomRegion, TComparisonFlags>, new()
		where TRomRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{
		private static readonly string[] AllowedFileExtensions = new[] { ".srm", ".comp", ".state", ".000", ".001", ".002", ".003", ".004", ".005", ".006", ".007", ".008", ".009" };

		/// <summary>
		/// Parses a list of string arguments into an <see cref="IOptions"/> instance
		/// </summary>
		/// <param name="args">The string arguments to be parsed</param>
		/// <returns>Returns an <see cref="IOptions"/> instance</returns>
		public virtual IOptions Parse(IReadOnlyList<string> args)
		{
			if (args.Count == 0) return new TOptions();

			const string compFileExtension = ".comp";
			var currentFilePath = args[0];
			var options = new TOptions { CurrenFilePath = currentFilePath };

			if (currentFilePath.IsNullOrEmpty())
				throw new ArgumentException(Resources.ErrorMissingPathArguments, nameof(options.CurrenFilePath));

			options.ExportDirectory = Path.GetDirectoryName(options.CurrenFilePath);

			var namelessParamCount = 1;
			// Check nameless params
			for (var i = 1; i < args.Count; ++i)
			{
				var value = args[i];
				if (value.StartsWith("--")) break;

				if (Enum.TryParse<TRomRegion>(value, true, out var romRegion))
				{
					options.GameRegion = romRegion;
					++namelessParamCount;
				}
				else if (File.Exists(value) || File.Exists(Path.Join(options.ExportDirectory, value)))
				{
					EnsureFullQualifiedPath(ref value);
		
					options.ComparisonFilePath = value;
					++namelessParamCount;
				}
			}

#if Check_FileExtensions
			if (!AllowedFileExtensions.Contains(Path.GetExtension(currentFilePath).ToLower()))
				throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.Current, options.CurrenFilePath, AllowedFileExtensions.Join()));
#endif

			options.ComparisonFilePath ??= currentFilePath + compFileExtension;
			
			for (var i = namelessParamCount; i < args.Count; i += 2)
			{
				var cmdName = args[i].ToLower();

				if (i == args.Count - 1)
					throw new ArgumentException(Resources.ErrorParamNameValueMissmatchTemplate.InsertArgs(cmdName));

				var value = args[i + 1];

				switch (cmdName)
				{
					case CmdOptions.BatchCommands:
						options.BatchCommands = value.IsNullOrEmpty() ? null : value;
						break;
					case CmdOptions.ComparisonFile:
						EnsureFullQualifiedPath(ref value);

						options.ComparisonFilePath = value;
						break;
					case CmdOptions.ExportDirectory:
						options.ExportDirectory = value;
						break;
					case CmdOptions.CurrentSaveSlot:
						options.CurrentFileSaveSlot = value.ParseSaveSlotId();
						break;
					case CmdOptions.ComparisonSaveSlot:
						options.ComparisonFileSaveSlot = value.ParseSaveSlotId();
						break;
					case CmdOptions.GameRegion:
						options.GameRegion = value.ParseEnum<TRomRegion>();
						break;
					case CmdOptions.ComparisonFlags:
						options.ComparisonFlags = value.ParseEnum<TComparisonFlags>();
						break;
					case CmdOptions.ColorizeOutput:
						options.ColorizeOutput = value.ParseToBool();
						break;
					case CmdOptions.UILanguage:
						options.UILanguage = value;
						break;
					case CmdOptions.ComparisonResultLanguage:
						options.ComparisonResultLanguage = value;
						break;
					case CmdOptions.ConfigFilePath:
						options.ConfigFilePath = value;
						break;
				}
			}

			if (options.ComparisonFilePath.IsNullOrEmpty())
				throw new ArgumentException(Resources.ErrorMissingPathArguments, nameof(options.ComparisonFilePath));

#if Check_FileExtensions
			if (!AllowedFileExtensions.Contains(Path.GetExtension(options.ComparisonFilePath).ToLower()))
				throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.Comparison,  options.ComparisonFilePath, AllowedFileExtensions.Join()));
#endif

			return options;

			void EnsureFullQualifiedPath(ref string value)
			{
				if (Path.GetDirectoryName(value) == string.Empty)
					value = Path.Join(Path.GetDirectoryName(options.CurrenFilePath), value);
			}
		}
	}
}