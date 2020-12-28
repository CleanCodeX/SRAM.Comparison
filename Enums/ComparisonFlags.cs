using System;
using Common.Shared.Min.Attributes;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Enums
{
	[Flags]
	public enum ComparisonFlags: uint
	{
		[DisplayNameLocalized(nameof(Res.SlotByteByByteComparison), typeof(Res))]
		SlotByteByByteComparison = 1 << 0,
			
		[DisplayNameLocalized(nameof(Res.NonSlotByteByByteComparison), typeof(Res))]
		NonSlotByteByByteComparison = 1 << 1
	}
}
