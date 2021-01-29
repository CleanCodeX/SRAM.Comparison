using System;
using System.IO;
using Common.Shared.Min.Extensions;
using IO.Helpers;

namespace SRAM.Comparison.Helpers
{
	public static class FilePathHelper
	{
		public const string SrmFileExtension = ".srm";
		public const string SavestateFileExtension = ".state";
		public const string CompFileExtension = ".comp";

		public static bool IsSaveFile(string file) => IsSrmFile(file) || IsSavestateFile(file);

		public static bool IsSrmFile(string file) => IsValidSrmExtension(GetSaveFileExtension(file));
		public static bool IsSavestateFile(string file) => IsValidSavestateFileExtension(GetSaveFileExtension(file));

		private static bool IsValidSavestateFileExtension(string? extension)
		{
			if (extension.IsNullOrEmpty()) return false;

			extension = extension.ToLower();
			var isNumber = int.TryParse(extension.Substring(1), out var number);

			return extension switch
			{
				SavestateFileExtension => true,
				_ when isNumber && number >= 0 && number <= 9 => true,
				_ => false
			};
		}
		private static bool IsValidSrmExtension(string? extension) => extension?.ToLower() == SrmFileExtension;

		private static string? GetSaveFileExtension(string? file)
		{
			if (file is null) return null;

			var extension = Path.GetExtension(file).ToLower();
			return extension switch
			{
				CompFileExtension => Path.GetExtension(file.Remove(CompFileExtension)!).ToLower()!,
				_ => extension
			};
		}

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
