using System;

namespace SramComparer
{
	/// <summary>Standard options implementation</summary>
	/// <inheritdoc cref="IOptions"/>
	public class Options<TGameRegion, TComparisonFlags> : IOptions
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{
		public string? BatchCommands { get; set; }
		public string? CurrentSramFilePath { get; set; }
		public string? ComparisonSramFilePath { get; set; }
		public string? ExportDirectory { get; set; }
		
		public TGameRegion GameRegion { get; set; }
		public int CurrentSramFileSaveSlot { get; set; }
		public int ComparisonSramFileSaveSlot { get; set; }
		public TComparisonFlags ComparisonFlags { get; set; }

		Enum IOptions.GameRegion => GameRegion;
		Enum IOptions.ComparisonFlags
		{
			get => ComparisonFlags;
			set => ComparisonFlags = (TComparisonFlags)value;
		}

		public bool ColorizeOutput { get; set; } = true;
		public string? SaveStateType { get; set; }
		public string? UILanguage { get; set; }
		public string? ComparisonResultLanguage { get; set; }
		public string? ConfigFilePath { get; set; }
	}
}