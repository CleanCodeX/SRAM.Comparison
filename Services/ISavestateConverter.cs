using System.IO;

namespace SRAM.Comparison.Services
{
	public interface ISavestateConverter
	{
		byte[]? ConvertStreamIfSavestate(IOptions options, in Stream stream, string? filePath);
	}
}