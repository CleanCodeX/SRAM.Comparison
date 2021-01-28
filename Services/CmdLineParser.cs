#define Check_FileExtensions

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Shared.Min.Extensions;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Extensions;
using SRAM.Comparison.Properties;
// ReSharper disable StaticMemberInGenericType

namespace SRAM.Comparison.Services
{
	/// <summary>
	/// Standard implementation of <see cref="ICmdLineParser"/>
	/// </summary>
	/// <typeparam name="TOptions">The options's type t be parsed into</typeparam>
	/// <typeparam name="TGameRegion">The game file's region enum</typeparam>
	public class CmdLineParser<TOptions, TGameRegion> : CmdLineParser<TOptions, TGameRegion, ComparisonFlags>
		where TOptions : Options<TGameRegion>, new()
		where TGameRegion : struct, Enum
	{ }

	/// <inheritdoc cref="CmdLineParser{TOptions,TGameRegion}"/>
	/// <typeparam name="TComparisonFlags">The game's comparison flags enum</typeparam>
	public class CmdLineParser<TOptions, TGameRegion, TComparisonFlags> : CmdLineParser<TOptions, TGameRegion, TComparisonFlags, ExportFlags>
		where TOptions : Options<TGameRegion, TComparisonFlags>, new()
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{ }

	/// <inheritdoc cref="CmdLineParser{TOptions,TGameRegion,TComparisonFlags}"/>
	/// <typeparam name="TExportFlags">The game's export flags enum</typeparam>
	public class CmdLineParser<TOptions, TGameRegion, TComparisonFlags, TExportFlags> : CmdLineParser<TOptions, TGameRegion, TComparisonFlags, TExportFlags, LogFlags>
		where TOptions : Options<TGameRegion, TComparisonFlags, TExportFlags>, new()
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
		where TExportFlags : struct, Enum
	{}

	/// <inheritdoc cref="CmdLineParser{TOptions,TGameRegion,TComparisonFlags,TExportFlags}"/>
	/// <typeparam name="TLogFlags">The game's comparison flags enum</typeparam>
	public class CmdLineParser<TOptions, TGameRegion, TComparisonFlags, TExportFlags, TLogFlags> : ICmdLineParser
		where TOptions : Options<TGameRegion, TComparisonFlags, TExportFlags, TLogFlags>, new()
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
		where TExportFlags : struct, Enum
		where TLogFlags : struct, Enum
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

				if (Enum.TryParse<TGameRegion>(value, true, out var romRegion))
				{
					options.GameRegion = romRegion;
					++namelessParamCount;
				}
				else if (File.Exists(value) || File.Exists(Path.Join(directory, value)))
				{
					options.ComparisonPath = value;
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
					case CmdOptions.ComparisonPath:
						options.ComparisonPath = value;
						break;
					case CmdOptions.ComparisonFlags:
						options.ComparisonFlags = value.ParseEnum<TComparisonFlags>();
						break;
					case CmdOptions.GameRegion:
						options.GameRegion = value.ParseEnum<TGameRegion>();
						break;
					case CmdOptions.ExportPath:
						options.ExportPath = value;
						break;
					case CmdOptions.ExportFlags:
						options.ExportFlags = value.ParseEnum<TExportFlags>();
						break;
					case CmdOptions.LogPath:
						options.LogPath = value;
						break;
					case CmdOptions.LogFlags:
						options.ExportFlags = value.ParseEnum<TLogFlags>();
						break;
					case CmdOptions.WatchFlags:
						options.FileWatchFlags = value.ParseEnum<FileWatchFlags>();
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
					case CmdOptions.AutoSave:
						options.AutoSave = value.ParseToBool();
						break;
					case CmdOptions.AutoWatch:
						options.AutoWatch = value.ParseToBool();
						break;
					case CmdOptions.ComparisonResultLanguage:
						options.ComparisonResultLanguage = value;
						break;
					case CmdOptions.ConfigPath:
						var configExtension = ".json";
						var extension = Path.GetExtension(value);
						if (extension.IsNotNullOrEmpty() && extension != configExtension)
							throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.Config, value, configExtension));

						options.ConfigPath = value;
						break;
					case { } when cmdName.StartsWith("--"):
						options.CustomOptions[cmdName[2..]] = value;

						break;
				}
			}

#if Check_FileExtensions
			if (options.ComparisonPath.IsNotNullOrEmpty() && !AllowedFileExtensions.Contains(Path.GetExtension(options.ComparisonPath!).ToLower()))
				throw new ArgumentException(Resources.ErrorInvalidFileExtensionTemplate.InsertArgs(Resources.CompComparison,  options.ComparisonPath, AllowedFileExtensions.Join()));
#endif

			return options;
		}
	}
}