namespace SramComparer.Services
{
	/// <summary>
	/// Set of possible command arguments
	/// </summary>
	public static class CmdOptions
	{
		public const string ComparisonFile = "--comp-file";
		public const string GameRegion = "--region";
		public const string BatchCommands = "--cmds";
		public const string CurrentSaveSlot = "--slot";
		public const string ComparisonSaveSlot = "--comp-slot";
		public const string ExportDirectory = "--exportdir";
		public const string ColorizeOutput = "--colorize";

		/// <summary> Flag for game specific flags</summary>
		public const string ComparisonFlags = "--flags";
	}
}