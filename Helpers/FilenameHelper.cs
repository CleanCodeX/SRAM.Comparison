using System;
using System.IO;
using Common.Shared.Min.Extensions;

namespace SRAM.Comparison.Helpers
{
	public static class FileNameHelper
	{
		public static string GenerateExportSaveFileName(string fileName)
		{
			var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
			return $"{fileName} # {normalizedTimestamp}.txt";
		}

		public static string GetComparisonFilePath(IOptions options)
		{
			var fileDirectory = Path.GetDirectoryName(options.ComparisonFilePath);
			if (fileDirectory.IsNullOrEmpty())
				fileDirectory = Path.GetDirectoryName(options.CurrentFilePath);

			var fileName = Path.GetFileName(options.ComparisonFilePath);
			if (fileName.IsNullOrEmpty())
				fileName = $"{Path.GetFileName(options.CurrentFilePath)}.comp";

			return Path.Join(fileDirectory, fileName);
		}
	}
}
