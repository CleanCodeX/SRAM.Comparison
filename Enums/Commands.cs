using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SramComparer.Helpers;
using Res = SramComparer.Properties.Resources;

namespace SramComparer.Enums
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Ausstehend>")]
	[JsonConverter(typeof(JsonStringEnumObjectConverter))]
	public enum Commands
	{
		[DisplayNameLocalized(nameof(Res.CmdListCommands), typeof(Res))]
		Help = 1, // Lists all in-app commands (alternatives: help, ?)

		[DisplayNameLocalized(nameof(Res.CmdConfig), typeof(Res))]
		Config, // shows current config and command line parameters

		[DisplayNameLocalized(nameof(Res.CmdGuideSrm), typeof(Res))]
		Guide_Srm, // Guide (srm) 

		[DisplayNameLocalized(nameof(Res.CmdGuideSavestate), typeof(Res))]
		Guide_Savestate, // Manual (savestate) 

		[DisplayNameLocalized(nameof(Res.EnumSlotByteByByteComparison), typeof(Res))]
		Sbc, // Enables or disables slot byte by byte comparison => compares the save slot area by by byte

		[DisplayNameLocalized(nameof(Res.EnumNonSlotByteByByteComparison), typeof(Res))]
		Nsbc, // Enables or disables non-slot byte by byte comparison => compares the non-save slot area byte by byte

		[DisplayNameLocalized(nameof(Res.CmdSetCurrentSrramFileSaveSlot), typeof(Res))]
		SetSlot, // Sets the save slot for the current file

		[DisplayNameLocalized(nameof(Res.CmdSetComparisonFileSaveSlot), typeof(Res))]
		SetSlot_Comp, // Sets the save slot for the comparison file

		[DisplayNameLocalized(nameof(Res.CmdCompareFiles), typeof(Res))]
		Compare, // Starts the comparison

		[DisplayNameLocalized(nameof(Res.CmdOverwriteComparisonFile), typeof(Res))]
		OverwriteComp, // Overwrite the comparison file with current file

		[DisplayNameLocalized(nameof(Res.CmdBackupCurrentFile), typeof(Res))]
		Backup, // Backups the current file

		[DisplayNameLocalized(nameof(Res.CmdBackupComparisonFile), typeof(Res))]
		Backup_Comp, // Backups the comparison file

		[DisplayNameLocalized(nameof(Res.CmdRestoreCurrentFile), typeof(Res))]
		Restore, // Restores the current file from previously created backup

		[DisplayNameLocalized(nameof(Res.CmdRestoreComparisonFile), typeof(Res))]
		Restore_Comp, // Restores the comparison file from previously created backup

		[DisplayNameLocalized(nameof(Res.CmdExportComparisonResult), typeof(Res))]
		Export, // Export the current comparison result as text file to export directory

		[DisplayNameLocalized(nameof(Res.CmdSaveCurrentFileWithOtherFileName), typeof(Res))]
		Transfer, // Transfer save file => Copies the current file to a different game name of a similar ROM type. E.g. copying between patched and unpatched ROM versions.

		[DisplayNameLocalized(nameof(Res.CmdClearOutput), typeof(Res))]
		Clear, // Clear the output window

		[DisplayNameLocalized(nameof(Res.CmdQuit), typeof(Res))]
		Quit, // Quit the app

		[DisplayNameLocalized(nameof(Res.CmdOffset), typeof(Res))]
		Offset, // Display a value at offset address

		[DisplayNameLocalized(nameof(Res.CmdEditOffset), typeof(Res))]
		EditOffset, // Saves a value to entered offset address

		[DisplayNameLocalized(nameof(Res.CmdLanguage), typeof(Res))]
		Lang, // Sets the UI language

		[DisplayNameLocalized(nameof(Res.CmdComparisonResultLanguage), typeof(Res))]
		Lang_Comp, // Sets the language for comparison results

		[DisplayNameLocalized(nameof(Res.CmdLoadConfig), typeof(Res))]
		LoadConfig, // Loads the current config from file

		[DisplayNameLocalized(nameof(Res.CmdSaveConfig), typeof(Res))]
		SaveConfig, // Saves current the config to file

		[DisplayNameLocalized(nameof(Res.CmdOpenConfig), typeof(Res))]
		OpenConfig, // opens the config file

		[DisplayNameLocalized(nameof(Res.CmdCreateKeyBindingFile), typeof(Res))]
		CreateBindings, // creates a custom key binding file

		[DisplayNameLocalized(nameof(Res.CmdOpenKeyBindingFile), typeof(Res))]
		OpenBindings, // opens the custom key binding file
	}
	
	public enum AlternateCommands
	{
		Guide = Commands.Guide_Srm,
		Manual = Commands.Guide_Srm,
		Oc = Commands.OverwriteComp,
		Cmds = Commands.Help,
		CmdLine = Commands.Config,
		Cfg = Commands.Config,
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
		O = Commands.Offset,
		EO = Commands.EditOffset,
		Load = Commands.LoadConfig,
		Save = Commands.SaveConfig,
		Open = Commands.OpenConfig,
	}
}
