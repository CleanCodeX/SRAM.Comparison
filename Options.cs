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
		[JsonIgnore]
		public string? BatchCommands { get; set; }
		public string? CurrenFilePath { get; set; }
		public string? ComparisonFilePath { get; set; }
		public string? ExportDirectory { get; set; }
		
		public TGameRegion GameRegion { get; set; }
		public int CurrentFileSaveSlot { get; set; }
		public int ComparisonFileSaveSlot { get; set; }
		public TComparisonFlags ComparisonFlags { get; set; }

		[JsonIgnore]
		Enum IOptions.GameRegion => GameRegion;
		[JsonIgnore]
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