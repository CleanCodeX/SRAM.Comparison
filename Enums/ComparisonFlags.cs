using System;
using Common.Shared.Min.Attributes;
using Res = SRAM.Comparison.Properties.Resources;

namespace SRAM.Comparison.Enums
{
	[Flags]
	public enum ComparisonFlags: uint
	{
		[DisplayNameLocalized(nameof(Res.EnumSlotByteComparison), typeof(Res))]
		SlotByteComparison = 0x1,
			
		[DisplayNameLocalized(nameof(Res.EnumNonSlotComparison), typeof(Res))]
		NonSlotComparison = 0x2,
			
		[DisplayNameLocalized(nameof(Res.EnumChecksumStatus), typeof(Res))]
		ChecksumStatus = 0x4
	}
}
