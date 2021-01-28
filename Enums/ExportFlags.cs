using System;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Helpers;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison.Enums
{
	[DisplayNameLocalized(nameof(Resources.EnumExportFlags), typeof(Resources))]
	[JsonConverter(typeof(JsonStringEnumObjectConverter))]
	[Flags]
	public enum ExportFlags : uint
	{
		[DisplayNameLocalized(nameof(Resources.EnumOpenExportFile), typeof(Resources))]
		OpenFile = 0x1,

		[DisplayNameLocalized(nameof(Resources.EnumSelectExportFile), typeof(Resources))]
		SelectFile = 0x2,

		[DisplayNameLocalized(nameof(Resources.EnumPromptExportFile), typeof(Resources))]
		PromptName = 0x4,

		[DisplayNameLocalized(nameof(Resources.EnumOverwriteCompFile), typeof(Resources))]
		OverwriteComp = 0x8,

		[DisplayNameLocalized(nameof(Resources.EnumDeleteCompFile), typeof(Resources))]
		DeleteComp = 0x10,
	}
}