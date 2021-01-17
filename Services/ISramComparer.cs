using SramCommons.Models;

namespace SramComparer.Services
{
	/// <summary>Interface for implementations</summary>
	/// <typeparam name="TSramFile">The SRAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The SRAM save slot structure</typeparam>
	public interface ISramComparer<in TSramFile, in TSaveSlot>
		where TSramFile : class, IMultiSegmentFile<TSaveSlot>
		where TSaveSlot : struct
	{
		/// <summary>Compares all games of SRAM structure</summary>
		/// <param name="currFile">The current SRAM file structure</param>
		/// <param name="compFile">The comparison SRAM file structure</param>
		/// <param name="options">The options to be used for all comparisons</param>
		/// <returns>Number of compared bytes changed</returns>
		int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);

		/// <summary>Compares all games of SRAM structure</summary>
		/// <param name="currSlot">The current SRAM save slot structure</param>
		/// <param name="compSlot">The comparison SRAM save slot structure</param>
		/// <param name="options">The options to be used for all comparisons</param>
		/// <returns>Number of compared bytes changed</returns>
		int CompareSaveSlot(TSaveSlot currSlot, TSaveSlot compSlot, IOptions options);
	}
}