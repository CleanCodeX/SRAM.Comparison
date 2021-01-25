using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison
{
	/// <summary>Common interface for comparing S-RAM files</summary>
	public interface IOptions
	{
		/// <summary>Optional list of commands which should be performed in a row</summary>
		string? BatchCommands { get; set; }

		/// <summary>Optional region of S-RAM file. If Empty, EnglishNtsc is used.</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumGameRegion), typeof(Resources))]
		Enum GameRegion { get; set; }

		/// <summary>The filepath to load the current S-RAM file from</summary>
		string? CurrentFilePath { get; set; }

		/// <summary>
		/// The filepath to load the comparison S-RAM file from. if <see cref="CurrentFilePath"/> is set, it will be automatically filled from that
		/// </summary>
		string? ComparisonPath { get; set; }

		/// <summary>Optional save slot specific options for comparisons</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumComparisonFlags), typeof(Resources))]
		Enum ComparisonFlags { get; set; }

		/// <summary>
		/// If set, this file or directory will be used for exporting comparisons, otherwise the directory of <see cref="CurrentFilePath" />
		/// </summary>
		string? ExportPath { get; set; }

		/// <summary>Options to control exporting the comparison result into a file</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumExportFlags), typeof(Resources))]
		Enum ExportFlags { get; set; }

		/// <summary>Custom options</summary>
		Dictionary<string, string> CustomOptions { get; set; } 

		/// <summary>
		/// Gets or sets if only a specific save slot of current-S-RAM file should be compared. If zero (default), all save slots will be compared.
		/// </summary>
		int CurrentFileSaveSlot { get; set; }

		/// <summary>
		/// Gets or sets if only a specific save slot of comparison-S-RAM file should be compared. If zero (default), same save slot(config) as current-S-RAM file will be compared.
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

		/// <summary>If set, this config file or directory will be used, otherwise the default file in the directory of <see cref="CurrentFilePath" /></summary>
		string? ConfigPath { get; set; }

		/// <summary>If set, this log file or directory will be used, otherwise the default file in the directory of <see cref="CurrentFilePath" /></summary>
		string? LogPath { get; set; }

		/// <summary>Options to control logging</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumLogFlags), typeof(Resources))]
		Enum LogFlags { get; set; }
	}
}