using System;

namespace SramComparer.Helpers
{
	public static class FilenameHelper
	{
		public static string GenerateExportFilename(string srmFilename)
		{
			var normalizedTimestamp = DateTime.Now.ToString("s").Replace(":", "_");
			return $"{srmFilename} # {normalizedTimestamp}.txt";
		}
	}
}
