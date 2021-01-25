namespace SRAM.Comparison.Services
{
	/// <summary>
	/// Set of possible command arguments
	/// </summary>
	public static class CmdOptions
	{
		public const string GameRegion = "--region";

		/// <summary> Flag for game specific flags</summary>
		public const string ComparisonFlags = "--comp-flags";
		public const string ComparisonPath = "--comp-path";
		public const string ComparisonResultLanguage = "--comp-lang";
		public const string ComparisonSaveSlot = "--comp-slot";

		public const string BatchCommands = "--batch-cmds";
		public const string ColorizeOutput = "--colorize";

		public const string CurrentSaveSlot = "--slot";

		public const string ExportPath = "--export-path";
		public const string ExportFlags = "--export-flags";

		public const string LogFlags = "--log-flags";
		public const string LogPath = "--log-path";

		public const string UILanguage = "--lang";
		public const string ConfigPath = "--config-path";
	}
}