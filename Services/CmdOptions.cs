namespace SramComparer.Services
{
	/// <summary>
	/// Set of possible command arguments
	/// </summary>
	public static class CmdOptions
	{
		public const string BatchCommands = "--cmds";
		public const string ComparisonFile = "--comp_file";
		public const string CurrentSaveSlot = "--gameslot";
		public const string ComparisonSaveSlot = "--comp_gameslot";
		public const string GameRegion = "--region";
		public const string ExportDirectory = "--exportdir";
		public const string ColorizeOutput = "--colorize";

		/// <summary> Flag for game specific flags</summary>
		public const string ComparisonFlags = "--flags";
	}
}