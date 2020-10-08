using System;

namespace SramComparer
{
	public class Options<TFileRegion, TComparisonFlags> : IOptions
		where TFileRegion : struct, Enum
		where TComparisonFlags : struct, Enum
	{
		public string? Commands { get; set; }
#nullable disable
		public string CurrentGameFilepath { get; set; }
		public string ComparisonGameFilepath { get; set; }
		public string ExportDirectory { get; set; }
#nullable restore

		public TFileRegion Region { get; set; }
		public int Game { get; set; }
		public int ComparisonGame { get; set; }
		public TComparisonFlags Flags;

		Enum IOptions.Region => Region;
		Enum IOptions.Flags
		{
			get => Flags;
			set => Flags = (TComparisonFlags)value;
		}
	}
}