using System;
using Common.Shared.Min.Attributes;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Enums
{
	[Flags]
	public enum ExportFlags 
	{
		[DisplayNameLocalized(nameof(Res.EnumOpenExportFile), typeof(Res))]
		Open = 0x1,

		[DisplayNameLocalized(nameof(Res.EnumSelectExportFile), typeof(Res))]
		Select = 0x2,

		[DisplayNameLocalized(nameof(Res.EnumPromptExportFile), typeof(Res))]
		Prompt = 0x4,

		[DisplayNameLocalized(nameof(Res.EnumOverwriteCompFile), typeof(Res))]
		OverwriteComp = 0x8,

		[DisplayNameLocalized(nameof(Res.EnumDeleteCompFile), typeof(Res))]
		DeleteComp = 0x10,
	}
}