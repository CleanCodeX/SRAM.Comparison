using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Enums;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison
{
	/// <inheritdoc />
	/// <typeparam name="TGameRegion">The game file's region enum</typeparam>
	public class Options<TGameRegion> : Options<TGameRegion, ComparisonFlags>
		where TGameRegion : struct, Enum
	{ }

	/// <inheritdoc />
	/// <typeparam name="TComparisonFlags">The game's comparison flags enum</typeparam>
	public class Options<TGameRegion, TComparisonFlags> : Options<TGameRegion, TComparisonFlags, ExportFlags>
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{ }

	/// <inheritdoc />
	/// <typeparam name="TExportFlags">The game's export flags enum</typeparam>
	public class Options<TGameRegion, TComparisonFlags, TExportFlags> : Options<TGameRegion, TComparisonFlags, TExportFlags, LogFlags>
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
		where TExportFlags : struct, Enum
	{ }

	/// <inheritdoc />
	/// <typeparam name="TLogFlags">The game's comparison flags enum</typeparam>
	public class Options<TGameRegion, TComparisonFlags, TExportFlags, TLogFlags> : IOptions
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
		where TExportFlags : struct, Enum
		where TLogFlags : struct, Enum
	{
		public string? BatchCommands { get; set; }
		public string? CurrentFilePath { get; set; }
		public string? ComparisonPath { get; set; }
		public string? ExportPath { get; set; }
		public TExportFlags ExportFlags { get; set; }
		public string? LogPath { get; set; }
		public bool AutoSave { get; set; }
		public TLogFlags LogFlags { get; set; }
		public Dictionary<string, string> CustomOptions { get; set; } = new();

		[DisplayNameLocalized(nameof(Resources.EnumGameRegion), typeof(Resources))]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TGameRegion GameRegion { get; set; }
		public int CurrentFileSaveSlot { get; set; }
		public int ComparisonFileSaveSlot { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TComparisonFlags ComparisonFlags { get; set; }

		Enum IOptions.GameRegion
		{
			get => GameRegion;
			set => GameRegion = (TGameRegion)value;
		}
		Enum IOptions.ComparisonFlags
		{
			get => ComparisonFlags;
			set => ComparisonFlags = (TComparisonFlags)value;
		}
		Enum IOptions.ExportFlags
		{
			get => ExportFlags;
			set => ExportFlags = (TExportFlags)value;
		}
		Enum IOptions.LogFlags
		{
			get => LogFlags;
			set => LogFlags = (TLogFlags)value;
		}
		Enum IOptions.FileWatchFlags
		{
			get => FileWatchFlags;
			set => FileWatchFlags = (FileWatchFlags)value;
		}

		public bool ColorizeOutput { get; set; } = true;
		public string? SavestateType { get; set; }
		public string? UILanguage { get; set; }
		public string? ComparisonResultLanguage { get; set; }
		public string? ConfigPath { get; set; }
		public bool AutoWatch { get; set; }
		public FileWatchFlags FileWatchFlags { get; set; }
	}
}