using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SramComparer.Properties;

namespace SramComparer
{
	/// <summary>Standard options implementation</summary>
	/// <inheritdoc cref="IOptions"/>
	public class Options<TGameRegion, TComparisonFlags, TExportFlags> : IOptions
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
		where TExportFlags : struct, Enum
	{
		public string? BatchCommands { get; set; }
		public string? CurrentFilePath { get; set; }
		public string? ComparisonFilePath { get; set; }
		public string? ExportDirectory { get; set; }
		public TExportFlags ExportFlags { get; set; }
		public Dictionary<string, string> Custom { get; set; } = new();

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

		public bool ColorizeOutput { get; set; } = true;
		public string? SavestateType { get; set; }
		public string? UILanguage { get; set; }
		public string? ComparisonResultLanguage { get; set; }

		public string? ConfigFilePath { get; set; }
	}
}