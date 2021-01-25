using System;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison.Enums
{
	[JsonConverter(typeof(JsonStringEnumObjectConverter))]
	[DisplayNameLocalized(nameof(Resources.EnumLogFlags), typeof(Resources))]
	[Flags]
	public enum LogFlags
	{
		[DisplayNameLocalized(nameof(IO.Properties.Resources.EnumNone), typeof(Resources))]
		None,
		[DisplayNameLocalized(nameof(Resources.EnumLogExport), typeof(Resources))]
		Export = 0x1,
		[DisplayNameLocalized(nameof(Resources.EnumLogComparison), typeof(Resources))]
		Comparison = 0x2
	}
}