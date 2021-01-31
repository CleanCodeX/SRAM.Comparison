using System;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison.Enums
{
	[DisplayNameLocalized(nameof(Resources.EnumComparisonFlags), typeof(Resources))]
	[JsonConverter(typeof(JsonStringEnumObjectConverter))]
	[Flags]
	public enum ComparisonFlags: uint
	{
		[DisplayNameLocalized(nameof(Resources.EnumSaveSlotByteComparison), typeof(Resources))]
		SlotByteComparison = 0x1,
			
		[DisplayNameLocalized(nameof(Resources.EnumNonSaveSlotComparison), typeof(Resources))]
		NonSlotComparison = 0x2,
			
		[DisplayNameLocalized(nameof(Resources.EnumChecksumStatus), typeof(Resources))]
		ChecksumStatus = 0x4,

		[DisplayNameLocalized(nameof(Resources.EnumOverwriteCompFile), typeof(Resources))]
		OverwriteComp = 0x8,

		[DisplayNameLocalized(nameof(Resources.EnumAutoExport), typeof(Resources))]
		AutoExport = 0x10,
	}
}
