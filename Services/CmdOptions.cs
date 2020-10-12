namespace SramComparer.Services
{
	/// <summary>
	/// Set of possible command arguments
	/// </summary>
	public static class CmdOptions
	{
		public const string Command = "--cmd";
		public const string ComparisonFile = "--comparison_file";
		public const string CurrentGame = "--game";
		public const string ComparisonGame = "--comparison_game";
		public const string Region = "--region";
		public const string Exportdir = "--exportdir";

		/// <summary> Flag for game specific flags</summary>
		public const string ComparisonFlags = "--flags";
	}
}