using System;
using System.IO;
using SramCommons.Models;
using SramComparer.Enums;

namespace SramComparer.Services
{
	/// <summary>Non generic interface for <see cref="CommandHandler{TSramFile,TSaveSlot}"/> implementations</summary>
	public interface ICommandHandler
	{
		bool RunCommand(string command, IOptions options, TextWriter? outStream = null);
	}

	/// <summary>Generic interface for <see cref="CommandHandler{TSramFile,TSaveSlot}"/> implementations</summary>
	/// <typeparam name="TSramFile">The SRAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The SRAM game structure</typeparam>
	public interface ICommandHandler<out TSramFile, out TSaveSlot> : ICommandHandler
		where TSramFile : SramFile, IMultiSegmentFile<TSaveSlot>
		where TSaveSlot : struct
	{
		int GetSaveSlotId(int maxGameId);

		void OverwriteComparisonFileWithCurrentFile(IOptions options);

		/// <summary>Backups or restores current or comparison sram file</summary>
		/// <param name="options">Options for getting file paths</param>
		/// <param name="fileKind">Whether to use current or comparison file as source</param>
		/// <param name="restore">Whether to backup or restore a file</param>
		void BackupSaveFile(IOptions options, SaveFileKind fileKind, bool restore = false);

		/// <summary>Inverts a flag. If it is set, it will be removed and vice versa.</summary>
		/// <param name="flags">The flags value which flag value will be added to or removed from</param>
		/// <param name="flag">The flag to set or remove</param>
		/// <returns></returns>
		Enum InvertIncludeFlag(in Enum flags, in Enum flag);

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.CurrentFilePath"/> and <see cref="IOptions.ComparisonFilePath"/> will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		void Compare<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.CurrentFilePath"/> and <see cref="IOptions.ComparisonFilePath"/> will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="output">The stream the output should be written to</param>
		void Compare<TComparer>(IOptions options, TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>
		/// Compares SRAM based on <para>options</para> and <para>currStream</para> and <para>compStream</para> params
		/// </summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="currStream"></param>
		/// <param name="compStream"></param>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="output">The stream the output should be written to</param>
		void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options, TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>
		/// Compares SRAM based on <para>options</para> and <para>currStream</para> and <para>compStream</para> params
		/// </summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="currStream"></param>
		/// <param name="compStream"></param>
		/// <param name="options">The options to be used for comparison</param>
		void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.ExportDirectory"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <returns>The generated filepath</returns>
		string ExportComparisonResult<TComparer>(IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>Compares SRAM based on <para>options</para>.<see cref="IOptions.ExportDirectory"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="filePath"></param>
		void ExportComparisonResult<TComparer>(IOptions options, string filePath)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();
	}
}