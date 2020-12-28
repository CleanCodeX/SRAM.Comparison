using System.Diagnostics.CodeAnalysis;
using Common.Shared.Min.Attributes;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Enums
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Ausstehend>")]
	public enum Commands
	{
		[DisplayNameLocalized(nameof(Res.CommandListCommands), typeof(Res))]
		cmd = 1, // Lists all in-app commands (alternatives: help, ?)

		[DisplayNameLocalized(nameof(Res.CommandSettings), typeof(Res))]
		s, // shows current (command line) Settings

		[DisplayNameLocalized(nameof(Res.CommandManual), typeof(Res))]
		m, // Manual (how to use)

		[DisplayNameLocalized(nameof(Res.SlotByteByByteComparison), typeof(Res))]
		asbc, // Flag Whole Save Slot => compare additionally the whole save slot area instead of only unknowns

		[DisplayNameLocalized(nameof(Res.NonSlotByteByByteComparison), typeof(Res))]
		nsbc, // Flag Non Save slot => compare additionally the non-save slot area 

		[DisplayNameLocalized(nameof(Res.CommandSetCurrentSrramFileSaveSlot), typeof(Res))]
		ss, // Set save slot for current-SRAM file

		[DisplayNameLocalized(nameof(Res.CommandSetComparisonSramFileSaveSlot), typeof(Res))]
		ssc, // Set Comparison save slot for comparison-SRAM file

		[DisplayNameLocalized(nameof(Res.CommandCompareFile), typeof(Res))]
		c, // Compare

		[DisplayNameLocalized(nameof(Res.CommandOverwriteComparisonSramFile), typeof(Res))]
		ow, // OverWrite the comparison-SRAM file with current-SRAM file

		[DisplayNameLocalized(nameof(Res.CommandBackupCurrentSramFile), typeof(Res))]
		b, // Backup the current-SRAM file

		[DisplayNameLocalized(nameof(Res.CommandBackupComparisonSramFile), typeof(Res))]
		bc, // backup the Comparison-SRAM file

		[DisplayNameLocalized(nameof(Res.CommandRestoreCurrentSramFile), typeof(Res))]
		r, // Restore the current-SRAM file from previously created backup

		[DisplayNameLocalized(nameof(Res.CommandRestoreComparisonSramFile), typeof(Res))]
		rc, // Restore the Comparison-SRAM file from previously created backup

		[DisplayNameLocalized(nameof(Res.CommandExportComparisonResult), typeof(Res))]
		e, // Export the current comparison result as text file to export directory

		[DisplayNameLocalized(nameof(Res.CommandSaveCurrentSramFileAsOtherFileName), typeof(Res))]
		ts, // Transfer SRAM => Copies the current-SRAM file to a different game name of a similar ROM type. Eg copying between patched and unpatched ROM versions.

		[DisplayNameLocalized(nameof(Res.CommandWipeOutput), typeof(Res))]
		w, // Wipe (clears) the output in command window

		[DisplayNameLocalized(nameof(Res.CommandQuit), typeof(Res))]
		q, // Quit

		[DisplayNameLocalized(nameof(Res.CommandOffsetValue), typeof(Res))]
		ov, // Display Offset Value => displays a value at entered offset address

		[DisplayNameLocalized(nameof(Res.CommandModifyOffsetValue), typeof(Res))]
		mov // Manipulate Offset Value => sets a value to entered offset addres
	}
	
	public enum AlternateCommands
	{
		cmds = Commands.cmd,
		help = Commands.m,
		cmdline = Commands.s,
		settings = cmdline,
		clear = Commands.w,
	}
}
