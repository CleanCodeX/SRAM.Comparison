using System;
using System.IO;
using Common.Shared.Min.Extensions;

namespace SRAM.Comparison.Helpers
{
	public static class FilePathHelper
	{
		public static string GenerateExportSaveFileName(string fileName)
		{
			var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
			return $"{fileName} # {normalizedTimestamp}.txt";
		}

		public static string GetComparisonFilePath(IOptions options)
		{
			var fileDirectory = Path.GetDirectoryName(options.ComparisonPath);
			if (fileDirectory.IsNullOrEmpty())
				fileDirectory = Path.GetDirectoryName(options.CurrentFilePath)!;

			EnsureDirectoryExists(fileDirectory);

			var fileName = Path.GetFileName(options.ComparisonPath);
			if (fileName.IsNullOrEmpty())
				fileName = $"{Path.GetFileName(options.CurrentFilePath)}.comp";

			return Path.Join(fileDirectory, fileName);
		}

		public static string GetLogFilePath(IOptions options, string logFileName)
		{
			var fileDirectory = Path.GetDirectoryName(options.LogPath);
			if (fileDirectory.IsNullOrEmpty())
				fileDirectory = options.ExportPath ?? Path.GetDirectoryName(options.CurrentFilePath)!;
			
			EnsureDirectoryExists(fileDirectory);

			var fileName = Path.GetFileName(options.LogPath);
			if (fileName.IsNullOrEmpty())
				fileName = logFileName;

			return Path.Join(fileDirectory, fileName);
		}

		public static void EnsureDirectoryExists(string fileDirectory)
		{
			if (!Directory.Exists(fileDirectory))
				Directory.CreateDirectory(fileDirectory!);
		}
	}
}
