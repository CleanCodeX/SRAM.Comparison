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
		[DisplayNameLocalized(nameof(Resources.BatchCommands), typeof(Resources))]
		string? BatchCommands { get; set; }

		/// <summary>Optional region of S-RAM file. If Empty, EnglishNtsc is used.</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumGameRegion), typeof(Resources))]
		Enum GameRegion { get; set; }

		/// <summary>The filepath to load the current S-RAM file from</summary>
		[DisplayNameLocalized(nameof(Resources.CurrentFilePath), typeof(Resources))]
		string? CurrentFilePath { get; set; }

		/// <summary>
		/// The filepath to load the comparison S-RAM file from. if <see cref="CurrentFilePath"/> is set, it will be automatically filled from that
		/// </summary>
		[DisplayNameLocalized(nameof(Resources.ComparisonPath), typeof(Resources))]
		string? ComparisonPath { get; set; }

		/// <summary>Optional save slot specific options for comparisons</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumComparisonFlags), typeof(Resources))]
		Enum ComparisonFlags { get; set; }

		/// <summary>
		/// If set, this file or directory will be used for exporting comparisons, otherwise the directory of <see cref="CurrentFilePath" />
		/// </summary>
		[DisplayNameLocalized(nameof(Resources.ExportPath), typeof(Resources))]
		string? ExportPath { get; set; }

		/// <summary>Options to control exporting the comparison result into a file</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumExportFlags), typeof(Resources))]
		Enum ExportFlags { get; set; }

		/// <summary>Custom options</summary>
		[DisplayNameLocalized(nameof(Resources.CustomOptions), typeof(Resources))]
		Dictionary<string, string> CustomOptions { get; set; }

		/// <summary>
		/// Gets or sets if only a specific save slot of current-S-RAM file should be compared. If zero (default), all save slots will be compared.
		/// </summary>
		[DisplayNameLocalized(nameof(Resources.CurrentFileSaveSlot), typeof(Resources))]
		int CurrentFileSaveSlot { get; set; }

		/// <summary>
		/// Gets or sets if only a specific save slot of comparison-S-RAM file should be compared. If zero (default), same save slot(config) as current-S-RAM file will be compared.
		/// </summary>
		[DisplayNameLocalized(nameof(Resources.ComparisonFileSaveSlot), typeof(Resources))]
		int ComparisonFileSaveSlot { get; set; }

		/// <summary>Optional flag whether to colorize the output or not</summary>
		[DisplayNameLocalized(nameof(Resources.ColorizeOutput), typeof(Resources))]
		bool ColorizeOutput { get; set; }

		/// <summary>Optional string which emulator savestate type is used</summary>
		[DisplayNameLocalized(nameof(Resources.SavestateType), typeof(Resources))]
		string? SavestateType { get; set; }

		/// <summary>The language in which the app should be displayed</summary>
		[DisplayNameLocalized(nameof(Resources.UILanguage), typeof(Resources))]
		string? UILanguage { get; set; }

		/// <summary>The language in which the comparison result should be displayed</summary>
		[DisplayNameLocalized(nameof(Resources.ComparisonResultLanguage), typeof(Resources))]
		string? ComparisonResultLanguage { get; set; }

		/// <summary>If set, this config file or directory will be used, otherwise the default file in the directory of <see cref="CurrentFilePath" /></summary>
		[DisplayNameLocalized(nameof(Resources.ConfigPath), typeof(Resources))]
		string? ConfigPath { get; set; }

		/// <summary>If set, this log file or directory will be used, otherwise the default file in the directory of <see cref="CurrentFilePath" /></summary>
		[DisplayNameLocalized(nameof(Resources.LogPath), typeof(Resources))]
		string? LogPath { get; set; }

		/// <summary>Whether to auto save changes or not</summary>
		[DisplayNameLocalized(nameof(Resources.AutoSave), typeof(Resources))]
		bool AutoSave { get; set; }

		/// <summary>Whether to watch for current file changes or not</summary>
		[DisplayNameLocalized(nameof(Resources.AutoWatch), typeof(Resources))]
		bool AutoWatch { get; set; }

		/// <summary>Options to control auto watch the current file</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumFileWatchFlags), typeof(Resources))]
		Enum FileWatchFlags { get; set; }

		/// <summary>Options to control logging</summary>
		[JsonConverter(typeof(JsonStringEnumObjectConverter))]
		[DisplayNameLocalized(nameof(Resources.EnumLogFlags), typeof(Resources))]
		Enum LogFlags { get; set; }
	}
}