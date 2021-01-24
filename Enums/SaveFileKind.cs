using Common.Shared.Min.Attributes;
using SRAM.Comparison.Properties;

namespace SRAM.Comparison.Enums
{
	public enum SaveFileKind
	{
		[DisplayNameLocalized(nameof(Resources.EnumCurrentFile), typeof(Resources))]
		CurrentFile,
		[DisplayNameLocalized(nameof(Resources.EnumComparisonFile), typeof(Resources))]
		ComparisonFile
	}
}