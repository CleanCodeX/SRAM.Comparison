using System.IO;
using IO.Models;

namespace SRAM.Comparison.Services
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
		int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options, TextWriter? output = null);

		///// <summary>Compares a single save slot</summary>
		///// <param name="currSlot">The current save slot</param>
		///// <param name="compSlot">The comparison save slot</param>
		///// <param name="options">The options to be used for comparison</param>
		///// <returns>Number of compared bytes changed</returns>
		//int CompareSaveSlot(TSaveSlot currSlot, TSaveSlot compSlot, IOptions options);
	}
}