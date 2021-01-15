using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SramComparer.Helpers;
using SramComparer.Properties;

namespace SramComparer
{
	/// <summary>Common interface for comparing SRAM files</summary>
	public interface IOptions
	{
		/// <summary>Optional list of commands which should be performed in a row</summary>
		string? BatchCommands { get; set; }

		/// <summary>Optional region of SRAM file. If Empty, EnglishNtsc is used.</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumGameRegion), typeof(Resources))]
		Enum GameRegion { get; set; }

		/// <summary>The filepath to load the current SRAM file from</summary>
		string? CurrentFilePath { get; set; }

		/// <summary>
		/// The filepath to load the comparison SRAM file from. if <see cref="CurrentFilePath"/> is set, it will be automatically filled from that
		/// </summary>
		string? ComparisonFilePath { get; set; }

		/// <summary>Optional save slot specific flags for comparisons</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		Enum ComparisonFlags { get; set; }

		/// <summary>
		/// If set, this directory will be used for exporting comparisons, otherwise the directory of <see cref="CurrentFilePath" />
		/// </summary>
		string? ExportDirectory { get; set; }

		/// <summary>Options to control exporting the comparison result into a file</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumExportFlags), typeof(Resources))]
		Enum ExportFlags { get; set; }

		/// <summary>Optional custom cmd params</summary>
		Dictionary<string, string> Custom { get; set; } 

		/// <summary>
		/// Gets or sets if only a specific save slot of current-SRAM file should be compared. If zero (default), all save slots will be compared.
		/// </summary>
		int CurrentFileSaveSlot { get; set; }

		/// <summary>
		/// Gets or sets if only a specific save slot of comparison-SRAM file should be compared. If zero (default), same save slot(config) as current-SRAM file will be compared.
		/// </summary>
		int ComparisonFileSaveSlot { get; set; }

		/// <summary>Optional flag whether to colorize the output or not</summary>
		bool ColorizeOutput { get; set; }

		/// <summary>Optional string which emulator savestate type is used</summary>
		string? SavestateType { get; set; }

		/// <summary>The language in which the app should be displayed</summary>
		string? UILanguage { get; set; }

		/// <summary>The language in which the comparison result should be displayed</summary>
		string? ComparisonResultLanguage { get; set; }

		/// <summary>If set, this config file will be used, otherwise the default file in the directory of <see cref="CurrentFilePath" /></summary>
		string? ConfigFilePath { get; set; }
	}
}