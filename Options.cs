using System;
using System.Text.Json.Serialization;

namespace SramComparer
{
	/// <summary>Standard options implementation</summary>
	/// <inheritdoc cref="IOptions"/>
	public class Options<TGameRegion, TComparisonFlags> : IOptions
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{
		public string? BatchCommands { get; set; }
		public string? CurrenFilePath { get; set; }
		public string? ComparisonFilePath { get; set; }
		public string? ExportDirectory { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TGameRegion GameRegion { get; set; }
		public int CurrentFileSaveSlot { get; set; }
		public int ComparisonFileSaveSlot { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TComparisonFlags ComparisonFlags { get; set; }

		Enum IOptions.GameRegion => GameRegion;
		Enum IOptions.ComparisonFlags
		{
			get => ComparisonFlags;
			set => ComparisonFlags = (TComparisonFlags)value;
		}

		public bool ColorizeOutput { get; set; } = true;
		public string? SavestateType { get; set; }
		public string? UILanguage { get; set; }
		public string? ComparisonResultLanguage { get; set; }

		[JsonIgnore]
		public string? ConfigFilePath { get; set; }
	}
}