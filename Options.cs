using System;

namespace SramComparer
{
	public class Options<TGameRegion, TComparisonFlags> : IOptions
		where TGameRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{
		public string? BatchCommands { get; set; }
		public string? CurrentSramFilepath { get; set; }
		public string? ComparisonSramFilepath { get; set; }
		public string? ExportDirectory { get; set; }

		public TGameRegion GameRegion { get; set; }
		public int CurrentSramFileSaveSlot { get; set; }
		public int ComparisonSramFileSaveSlot { get; set; }
		public TComparisonFlags ComparisonFlags;

		Enum IOptions.GameRegion => GameRegion;
		Enum IOptions.ComparisonFlags
		{
			get => ComparisonFlags;
			set => ComparisonFlags = (TComparisonFlags)value;
		}

		public bool ColorizeOutput { get; set; } = true;
	}
}