using System;
using System.IO;
using Common.Shared.Min.Extensions;
using IO.Helpers;

namespace SRAM.Comparison.Helpers
{
	public static class FilePathHelper
	{
		public static string GenerateExportSaveFileName(in string fileName)
		{
			var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
			return $"{fileName} # {normalizedTimestamp}.txt";
		}

		public static string GetExportFilePath(in IOptions options, in string? exportFileName = null)
		{
			string filePath;

			if (exportFileName is null && Path.GetFileNameWithoutExtension(options.ExportPath) is not null)
			{
				var directoryPath = Path.GetDirectoryName(options.ExportPath) ?? Path.GetDirectoryName(options.CurrentFilePath);
				var fileName = Path.GetFileNameWithoutExtension(options.ExportPath);

				filePath = Path.Join(directoryPath, fileName);
			}
			else
			{
				var fileName = exportFileName;
				if (fileName.IsNullOrWhiteSpace())
				{
					fileName = Path.GetFileNameWithoutExtension(options.CurrentFilePath)!;
					fileName = GenerateExportSaveFileName(fileName);
				}

				var directoryPath = options.ExportPath ?? Path.GetDirectoryName(options.CurrentFilePath);

				filePath = Path.Join(directoryPath, fileName);
			}

			return filePath;
		}

		public static string GetComparisonFilePath(in IOptions options)
		{
			var directory = Path.GetDirectoryName(options.ComparisonPath);
			if (directory.IsNullOrEmpty())
				directory = Path.GetDirectoryName(options.CurrentFilePath)!;

			DirectoryHelper.EnsureDirectoryExists(directory);

			var fileName = Path.GetFileName(options.ComparisonPath);
			if (fileName.IsNullOrEmpty())
				fileName = $"{Path.GetFileName(options.CurrentFilePath)}.comp";

			return Path.Join(directory, fileName);
		}

		public static string GetLogFilePath(in IOptions options, in string logFileName)
		{
			var directory = Path.GetDirectoryName(options.LogPath);
			if (directory.IsNullOrEmpty())
				directory = options.ExportPath ?? Path.GetDirectoryName(options.CurrentFilePath)!;
			
			DirectoryHelper.EnsureDirectoryExists(directory);

			var fileName = Path.GetFileName(options.LogPath);
			if (fileName.IsNullOrEmpty())
				fileName = logFileName;

			return Path.Join(directory, fileName);
		}
	}
}
