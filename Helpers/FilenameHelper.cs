using System;

namespace SramComparer.Helpers
{
	public static class FileNameHelper
	{
		public static string GenerateExportSaveFileName(string fileName)
		{
			var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
			return $"{fileName} # {normalizedTimestamp}.txt";
		}
	}
}
