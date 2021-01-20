using System;
using Common.Shared.Min.Attributes;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Enums
{
	[Flags]
	public enum ComparisonFlags: uint
	{
		[DisplayNameLocalized(nameof(Res.EnumSlotByteComparison), typeof(Res))]
		SlotByteByByteComparison = 0x1,
			
		[DisplayNameLocalized(nameof(Res.EnumNonSlotByteComparison), typeof(Res))]
		NonSlotByteByByteComparison = 0x2,
			
		[DisplayNameLocalized(nameof(Res.EnumNonSlotByteComparison), typeof(Res))]
		HideValidationStatus = 0x4
	}
}
