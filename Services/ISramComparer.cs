using SramCommons.Models;

namespace SramComparer.Services
{
	/// <summary>Interface for implementations</summary>
	/// <typeparam name="TSramFile">The SRAM file structure</typeparam>
	/// <typeparam name="TSramGame">The SRAM game structure</typeparam>
	public interface ISramComparer<in TSramFile, in TSramGame>
		where TSramFile : SramFile, ISramFile<TSramGame>
		where TSramGame : struct
	{
		/// <summary>Compares all games of SRAM structure</summary>
		/// <param name="currFile">The current SRAM file structure</param>
		/// <param name="compFile">The comparison SRAM file structure</param>
		/// <param name="options">The options to be used for all comparisons</param>
		/// <returns>Number of compared bytes changed</returns>
		int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);

		/// <summary>Compares all games of SRAM structure</summary>
		/// <param name="currGame">The current SRAM game structure</param>
		/// <param name="compGame">The comparison SRAM game structure</param>
		/// <param name="options">The options to be used for all comparisons</param>
		/// <returns>Number of compared bytes changed</returns>
		int CompareGame(TSramGame currGame, TSramGame compGame, IOptions options);
	}
}