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
		Help = 1, // Lists all in-app commands (alternatives: help, ?)

		[DisplayNameLocalized(nameof(Res.CommandConfig), typeof(Res))]
		Config, // shows current config and command line parameters

		[DisplayNameLocalized(nameof(Res.CommandGuideSrm), typeof(Res))]
		Guide_Srm, // Guide (srm) 

		[DisplayNameLocalized(nameof(Res.CommandGuideSavestate), typeof(Res))]
		Guide_Savestate, // Manual (savestate) 

		[DisplayNameLocalized(nameof(Res.SlotByteByByteComparison), typeof(Res))]
		Sbc, // Enables or disables slot byte by byte comparison => compares the save slot area by by byte

		[DisplayNameLocalized(nameof(Res.NonSlotByteByByteComparison), typeof(Res))]
		Nsbc, // Enables or disables non-slot byte by byte comparison => compares the non-save slot area byte by byte

		[DisplayNameLocalized(nameof(Res.CommandSetCurrentSrramFileSaveSlot), typeof(Res))]
		SetSlot, // Sets the save slot for the current file

		[DisplayNameLocalized(nameof(Res.CommandSetComparisonFileSaveSlot), typeof(Res))]
		SetSlot_Comp, // Sets the save slot for the comparison file

		[DisplayNameLocalized(nameof(Res.CommandCompareFiles), typeof(Res))]
		Compare, // Starts the comparison

		[DisplayNameLocalized(nameof(Res.CommandOverwriteComparisonFile), typeof(Res))]
		OverwriteComp, // Overwrite the comparison file with current file

		[DisplayNameLocalized(nameof(Res.CommandBackupCurrentFile), typeof(Res))]
		Backup, // Backups the current file

		[DisplayNameLocalized(nameof(Res.CommandBackupComparisonFile), typeof(Res))]
		Backup_Comp, // Backups the comparison file

		[DisplayNameLocalized(nameof(Res.CommandRestoreCurrentFile), typeof(Res))]
		Restore, // Restores the current file from previously created backup

		[DisplayNameLocalized(nameof(Res.CommandRestoreComparisonFile), typeof(Res))]
		Restore_Comp, // Restores the comparison file from previously created backup

		[DisplayNameLocalized(nameof(Res.CommandExportComparisonResult), typeof(Res))]
		Export, // Export the current comparison result as text file to export directory

		[DisplayNameLocalized(nameof(Res.CommandSaveCurrentFileWithOtherFileName), typeof(Res))]
		Transfer, // Transfer save file => Copies the current file to a different game name of a similar ROM type. E.g. copying between patched and unpatched ROM versions.

		[DisplayNameLocalized(nameof(Res.CommandClearOutput), typeof(Res))]
		Clear, // Clear the output window

		[DisplayNameLocalized(nameof(Res.CommandQuit), typeof(Res))]
		Quit, // Quit the app

		[DisplayNameLocalized(nameof(Res.CommandOffset), typeof(Res))]
		Offset, // Display a value at offset address

		[DisplayNameLocalized(nameof(Res.CommandEditOffset), typeof(Res))]
		EditOffset, // Saves a value to entered offset address

		[DisplayNameLocalized(nameof(Res.CommandLanguage), typeof(Res))]
		Lang, // Sets the UI language

		[DisplayNameLocalized(nameof(Res.CommandComparisonResultLanguage), typeof(Res))]
		Lang_Comp, // Sets the language for comparison results

		[DisplayNameLocalized(nameof(Res.CommandLoadConfig), typeof(Res))]
		LoadConfig, // Loads the current config from file

		[DisplayNameLocalized(nameof(Res.CommandSaveConfig), typeof(Res))]
		SaveConfig, // Saves current the config to file
	}
	
	public enum AlternateCommands
	{
		Guide = Commands.Guide_Srm,
		Manual = Commands.Guide_Srm,
		Oc = Commands.OverwriteComp,
		Cmds = Commands.Help,
		CmdLine = Commands.Config,
		Q = Commands.Quit,
		B = Commands.Backup,
		BC = Commands.Backup_Comp,
		R = Commands.Restore,
		RC = Commands.Restore_Comp,
		T = Commands.Transfer,
		Cls = Commands.Clear,
		W = Commands.Clear,
		L = Commands.Lang,
		LC = Commands.Lang_Comp,
		C = Commands.Compare,
		Ov = Commands.Offset,
		Eov = Commands.EditOffset,
		Load = Commands.LoadConfig,
		Save = Commands.SaveConfig,
	}
}
