namespace SramComparer.Services
{
	/// <summary>
	/// Set of possible command arguments
	/// </summary>
	public static class CmdOptions
	{
		public const string ComparisonFile = "--comp-file";
		public const string GameRegion = "--region";
		public const string BatchCommands = "--batch-cmds";
		public const string CurrentSaveSlot = "--slot";
		public const string ComparisonSaveSlot = "--comp-slot";
		public const string ExportDirectory = "--export-dir";
		public const string ColorizeOutput = "--colorize";

		/// <summary> Flag for game specific flags</summary>
		public const string ComparisonFlags = "--flags";

		public const string UILanguage = "--lang";
		public const string ComparisonResultLanguage = "--comp-lang";

		public const string ConfigFilePath = "--config-file";
	}
}