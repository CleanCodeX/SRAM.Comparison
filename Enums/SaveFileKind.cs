using Common.Shared.Min.Attributes;
using SramComparer.Properties;

namespace SramComparer.Enums
{
	public enum SaveFileKind
	{
		[DisplayNameLocalized(nameof(Resources.EnumCurrentFile), typeof(Resources))]
		CurrentFile,
		[DisplayNameLocalized(nameof(Resources.EnumComparisonFile), typeof(Resources))]
		ComparisonFile
	}
}