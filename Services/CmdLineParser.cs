#define Check_FileExtensions

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using SramComparer.Extensions;
using SramComparer.Properties;
// ReSharper disable StaticMemberInGenericType

namespace SramComparer.Services
{
	/// <summary>
	/// Standard implementation of <see cref="ICmdLineParser"/>
	/// </summary>
	/// <typeparam name="TOptions">The options's type t be parsed into</typeparam>
	/// <typeparam name="TRomRegion">The game file's region enum</typeparam>
	/// <typeparam name="TExportOptions">The game's export options flags enum</typeparam>
	/// <typeparam name="TComparisonFlags">The game's comparison flags enum</typeparam>
	public class CmdLineParser<TOptions, TRomRegion, TComparisonFlags, TExportOptions> : ICmdLineParser
		where TOptions : Options<TRomRegion, TComparisonFlags, TExportOptions>, new()
		where TRomRegion : struct, Enum
		where TComparisonFlags : struct, Enum
		where TExportOptions : struct, Enum
	{
		private static readonly string[] AllowedFileExtensions = { ".srm", ".comp", ".state", ".000", ".001", ".002", ".003", ".004", ".005", ".006", ".007", ".008", ".009" };

		/// <inheritdoc cref="ICmdLineParser.Parse(IReadOnlyList{string})"/>
		public virtual IOptions Parse(IReadOnlyList<string> args) => Parse(args, new TOptions());

		/// <inheritdoc cref="ICmdLineParser.Parse(IReadOnlyList{string}, IOptions)"/>
		public virtual IOptions Parse(IReadOnlyList<string> args, IOptions options)
		{
			if (args.Count == 0) return options;

			var currentFilePath = args[0];
			options.CurrentFilePath = currentFilePath;

			if (currentFilePath.IsNullOrEmpty())
				throw new ArgumentException(Resources.ErrorMissingPathArguments, nameof(options.CurrentFilePath));

			var directory = Path.GetDirectoryName(currentFilePath);

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
				else if (File.Exists(value) || File.Exists(Path.Join(directory, value)))
				{
					options.ComparisonFilePath = value;
					++namelessParamCount;
				}
			}

#if Check_FileExtensions
			if (!AllowedFileExtensions.Contains(Path.GetExtension(currentFilePath).ToLower()))
				throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.CompCurrent, options.CurrentFilePath, AllowedFileExtensions.Join()));
#endif

			for (var i = namelessParamCount; i < args.Count; i += 2)
			{
				var cmdName = args[i].ToLower();

				if (i == args.Count - 1)
					throw new ArgumentException(Resources.ErrorParamNameValueMissmatchTemplate.InsertArgs(cmdName));

				var value = args[i + 1];

				switch (cmdName)
				{
					case "":
						continue;
					case CmdOptions.BatchCommands:
						options.BatchCommands = value.IsNullOrEmpty() ? null : value;
						break;
					case CmdOptions.ComparisonFile:
						options.ComparisonFilePath = value;
						break;
					case CmdOptions.ComparisonFlags:
						options.ComparisonFlags = value.ParseEnum<TComparisonFlags>();
						break;
					case CmdOptions.GameRegion:
						options.GameRegion = value.ParseEnum<TRomRegion>();
						break;
					case CmdOptions.ExportDirectory:
						options.ExportDirectory = value;
						break;
					case CmdOptions.ExportFlags:
						options.ExportFlags = value.ParseEnum<TExportOptions>();
						break;
					case CmdOptions.CurrentSaveSlot:
						options.CurrentFileSaveSlot = value.ParseSaveSlotId();
						break;
					case CmdOptions.ComparisonSaveSlot:
						options.ComparisonFileSaveSlot = value.ParseSaveSlotId();
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
						var configExtension = ".json";
						var extension = Path.GetExtension(value);
						if (extension.IsNotNullOrEmpty() && extension != configExtension)
							throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.Config, value, configExtension));

						options.ConfigFilePath = value;
						break;
					case { } when cmdName.StartsWith("--"):
						options.Custom[cmdName.Substring(2)] = value;

						break;
				}
			}

#if Check_FileExtensions
			if (options.ComparisonFilePath.IsNotNullOrEmpty() && !AllowedFileExtensions.Contains(Path.GetExtension(options.ComparisonFilePath!).ToLower()))
				throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.CompComparison,  options.ComparisonFilePath, AllowedFileExtensions.Join()));
#endif

			return options;
		}
	}
}