using System;
using System.IO;
using IO.Models;
using SRAM.Comparison.Enums;

namespace SRAM.Comparison.Services
{
	/// <summary>Non generic interface for <see cref="CommandHandler{TSramFile,TSaveSlot}"/> implementations</summary>
	public interface ICommandHandler
	{
		bool RunCommand(string command, IOptions options, TextWriter? outStream = null);
	}

	/// <summary>Generic interface for <see cref="CommandHandler{TSramFile,TSaveSlot}"/> implementations</summary>
	/// <typeparam name="TSramFile">The S-RAM file structure</typeparam>
	/// <typeparam name="TSaveSlot">The S-RAM game structure</typeparam>
	public interface ICommandHandler<out TSramFile, out TSaveSlot> : ICommandHandler
		where TSramFile : class, IMultiSegmentFile<TSaveSlot>
		where TSaveSlot : struct
	{
		int GetSaveSlotId(in int maxGameId);

		void OverwriteComparisonFileWithCurrentFile(in IOptions options);

		/// <summary>Backups or restores current or comparison sram file</summary>
		/// <param name="options">Options for getting file paths</param>
		/// <param name="fileKind">Whether to use current or comparison file as source</param>
		/// <param name="restore">Whether to backup or restore a file</param>
		void BackupSaveFile(in IOptions options, in SaveFileKind fileKind, in bool restore = false);

		/// <summary>Inverts a flag. If it is set, it will be removed and vice versa.</summary>
		/// <param name="flags">The flags value which flag value will be added to or removed from</param>
		/// <param name="flag">The flag to set or remove</param>
		/// <returns></returns>
		Enum InvertIncludeFlag(in Enum flags, in Enum flag);

		/// <summary>Compares S-RAM based on <para>options</para>.<see cref="IOptions.CurrentFilePath"/> and <see cref="IOptions.ComparisonPath"/> will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		int Compare<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>Compares S-RAM based on <para>options</para>.<see cref="IOptions.CurrentFilePath"/> and <see cref="IOptions.ComparisonPath"/> will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="output">The stream the output should be written to</param>
		int Compare<TComparer>(in IOptions options, in TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>
		/// Compares S-RAM based on <para>options</para> and <para>currStream</para> and <para>compStream</para> params
		/// </summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="currStream"></param>
		/// <param name="compStream"></param>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="output">The stream the output should be written to</param>
		int Compare<TComparer>(Stream currStream, Stream compStream, in IOptions options, in TextWriter output)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>
		/// Compares S-RAM based on <para>options</para> and <para>currStream</para> and <para>compStream</para> params
		/// </summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="currStream"></param>
		/// <param name="compStream"></param>
		/// <param name="options">The options to be used for comparison</param>
		int Compare<TComparer>(Stream currStream, Stream compStream, in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>Compares S-RAM based on <para>options</para>.<see cref="IOptions.ExportPath"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <returns>The file path to export file or null if no export was performed (e.g. when no bytes changed)</returns>
		string? ExportCompResult<TComparer>(in IOptions options)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();

		/// <summary>Compares S-RAM based on <para>options</para>.<see cref="IOptions.ExportPath"/> and a generated filename based on current timestamp will be used</summary>
		/// <typeparam name="TComparer">The type of compare which should be used</typeparam>
		/// <param name="options">The options to be used for comparison</param>
		/// <param name="filePath">The file path to save the comparison result</param>
		/// <returns>The file path to export file or null if no export was performed (e.g. when no bytes changed)</returns>
		string? ExportCompResult<TComparer>(in IOptions options, in string filePath)
			where TComparer : ISramComparer<TSramFile, TSaveSlot>, new();
	}
}