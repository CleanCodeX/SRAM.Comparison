using SramCommons.Models;

namespace SramComparer.Services
{
	/// <summary>Interface for implementations</summary>
	/// <typeparam name="TSramFile">The S-RAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The S-RAM save slot structure</typeparam>
	public interface ISramComparer<in TSramFile, in TSaveSlot>
		where TSramFile : class, IMultiSegmentFile<TSaveSlot>
		where TSaveSlot : struct
	{
		/// <summary>Compares all games of S-RAM structure</summary>
		/// <param name="currFile">The current S-RAM file structure</param>
		/// <param name="compFile">The comparison S-RAM file structure</param>
		/// <param name="options">The options to be used for all comparisons</param>
		/// <returns>Number of compared bytes changed</returns>
		int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);

		/// <summary>Compares all games of S-RAM structure</summary>
		/// <param name="currSlot">The current S-RAM save slot structure</param>
		/// <param name="compSlot">The comparison S-RAM save slot structure</param>
		/// <param name="options">The options to be used for all comparisons</param>
		/// <returns>Number of compared bytes changed</returns>
		int CompareSaveSlot(TSaveSlot currSlot, TSaveSlot compSlot, IOptions options);
	}
}