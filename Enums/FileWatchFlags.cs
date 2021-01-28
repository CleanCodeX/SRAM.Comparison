using System;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison.Enums
{
	[DisplayNameLocalized(nameof(Resources.EnumFileWatchFlags), typeof(Resources))]
	[JsonConverter(typeof(JsonStringEnumObjectConverter))]
	[Flags]
	public enum FileWatchFlags : uint
	{
		[DisplayNameLocalized(nameof(Resources.EnumOverwriteCompFile), typeof(Resources))]
		OverwriteComp = 0x1,

		[DisplayNameLocalized(nameof(Resources.EnumAutoExport), typeof(Resources))]
		AutoExport = 0x2,
	}
}