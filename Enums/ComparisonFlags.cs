using System;
using Common.Shared.Min.Attributes;
using SramComparer.Properties;

namespace SramComparer.Enums
{
	[Flags]
	public enum ComparisonFlags: uint
	{
		[DisplayNameLocalized(nameof(Resources.CommandDoSlotAllBytesComparison), typeof(Resources))]
		SlotAllBytesComparison = 1 << 0,
			
		[DisplayNameLocalized(nameof(Resources.CommandDoNonSlotBytesComparison), typeof(Resources))]
		NonSlotBytesComparison = 1 << 1
	}
}
