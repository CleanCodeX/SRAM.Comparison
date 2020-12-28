using System;
using Common.Shared.Min.Attributes;
using SramComparer.Properties;

namespace SramComparer.Enums
{
	[Flags]
	public enum ComparisonFlags: uint
	{
		[DisplayNameLocalized(nameof(Resources.CommandDoSlotAllBytesComparison), typeof(Resources))]
		AddSlotByteComparison = 1 << 0,
			
		[DisplayNameLocalized(nameof(Resources.CommandDoNonSlotBytesComparison), typeof(Resources))]
		AddNonSlotByteComparison = 1 << 1
	}
}
