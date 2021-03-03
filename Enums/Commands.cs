using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Common.Shared.Min.Attributes;
using SRAM.Comparison.Helpers;
using Res = SRAM.Comparison.Properties.Resources;

namespace SRAM.Comparison.Enums
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
		SrmGuide, // Guide (srm) 

		[DisplayNameLocalized(nameof(Res.CmdGuideSavestate), typeof(Res))]
		SavestateGuide, // Manual (savestate) 

		[DisplayNameLocalized(nameof(Res.EnumSaveSlotByteComparison), typeof(Res))]
		SlotByteComp, // Enables or disables slot byte by byte comparison => compares the save slot area by by byte

		[DisplayNameLocalized(nameof(Res.EnumNonSaveSlotComparison), typeof(Res))]
		NonSlotComp, // Enables or disables non-slot byte by byte comparison => compares the non-save slot area byte by byte

		[DisplayNameLocalized(nameof(Res.CmdSetCurrentSrramFileSaveSlot), typeof(Res))]
		SetSlot, // Sets the save slot for the current file

		[DisplayNameLocalized(nameof(Res.CmdSetComparisonFileSaveSlot), typeof(Res))]
		SetCompSlot, // Sets the save slot for the comparison file

		[DisplayNameLocalized(nameof(Res.CmdShowChecksumStatus), typeof(Res))]
		ChecksumStatus, // Shows save slot checksum status

		[DisplayNameLocalized(nameof(Res.CmdCompareFiles), typeof(Res))]
		Compare, // Starts the comparison

		[DisplayNameLocalized(nameof(Res.EnumOverwriteCompFile), typeof(Res))]
		OverwriteComp, // Overwrite the comparison file with current file

		[DisplayNameLocalized(nameof(Res.CmdBackupCurrentFile), typeof(Res))]
		Backup, // Backups the current file

		[DisplayNameLocalized(nameof(Res.CmdBackupComparisonFile), typeof(Res))]
		BackupComp, // Backups the comparison file

		[DisplayNameLocalized(nameof(Res.CmdRestoreCurrentFile), typeof(Res))]
		Restore, // Restores the current file from previously created backup

		[DisplayNameLocalized(nameof(Res.CmdRestoreComparisonFile), typeof(Res))]
		RestoreComp, // Restores the comparison file from previously created backup

		[DisplayNameLocalized(nameof(Res.CmdExportCompResultResult), typeof(Res))]
		ExportCompResult, // Export the current comparison result as text file to export directory

		[DisplayNameLocalized(nameof(Res.CmdExportCompResultResultOpen), typeof(Res))]
		ExportCompResultOpen, // Export the current comparison result as text file to export directory and opens the file

		[DisplayNameLocalized(nameof(Res.CmdExportCompResultResultSelect), typeof(Res))]
		ExportCompResultSelect, // Export the current comparison result as text file to export directory and selects the file in explorer

		[DisplayNameLocalized(nameof(Res.EnumExportFlags), typeof(Res))]
		ExportFlags, // Export flags

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
		CompLang, // Sets the language for comparison results

		[DisplayNameLocalized(nameof(Res.CmdLoadConfig), typeof(Res))]
		LoadConfig, // Loads the current config from file

		[DisplayNameLocalized(nameof(Res.CmdSaveConfig), typeof(Res))]
		SaveConfig, // Saves current the config to file

		[DisplayNameLocalized(nameof(Res.CmdConfigAutoLoadOn), typeof(Res))]
		AutoLoadOn, // Enables config auto loading

		[DisplayNameLocalized(nameof(Res.CmdConfigAutoLoadOff), typeof(Res))]
		AutoLoadOff, // Disables config auto loading

		[DisplayNameLocalized(nameof(Res.CmdConfigAutoSaveOn), typeof(Res))]
		AutoSaveOn, // Enables config auto saving

		[DisplayNameLocalized(nameof(Res.CmdConfigAutoSaveOff), typeof(Res))]
		AutoSaveOff, // Disables config auto saving

		[DisplayNameLocalized(nameof(Res.CmdOpenConfig), typeof(Res))]
		OpenConfig, // opens the config file

		[DisplayNameLocalized(nameof(Res.CmdCreateKeyBindingFile), typeof(Res))]
		CreateBindings, // creates a custom key binding file

		[DisplayNameLocalized(nameof(Res.CmdOpenKeyBindingFile), typeof(Res))]
		OpenBindings, // opens the custom key binding file

		[DisplayNameLocalized(nameof(Res.CmdOpenLog), typeof(Res))]
		OpenLog, // opens the log file

		[DisplayNameLocalized(nameof(Res.CmdWatchCurrentFile), typeof(Res))]
		WatchFile, // starts watching for file changes

		[DisplayNameLocalized(nameof(Res.CmdUnwatchCurrentFile), typeof(Res))]
		UnwatchFile, // stops watching for file changes

		[DisplayNameLocalized(nameof(Res.EnumFileWatchFlags), typeof(Res))]
		FileWatchFlags, // File watch flags

		[DisplayNameLocalized(nameof(Res.EnumComparisonFlags), typeof(Res))]
		ComparisonFlags, // Comparison flags

		[DisplayNameLocalized(nameof(Res.EnumLogFlags), typeof(Res))]
		LogFlags, // Log flags

		[DisplayNameLocalized(nameof(Res.EnumShowSaveSlotSummary), typeof(Res))]
		ShowSlotSummary, // Slot summary

		[DisplayNameLocalized(nameof(Res.CmdExportSaveSlotSummary), typeof(Res))]
		ExportSlotSummary, // Export the slot's summary as text file

		[DisplayNameLocalized(nameof(Res.CmdCheckForUpdates), typeof(Res))]
		CheckForUpdate, // checks for updates

		[DisplayNameLocalized(nameof(Res.CmdApplyLatestUpdate), typeof(Res))]
		Update, // automatically downloads and applies a new update if available

		[DisplayNameLocalized(nameof(Res.CmdEnableDailyUpdateCheck), typeof(Res))]
		EnableDailyUpdateCheck, // enables daily update check

		[DisplayNameLocalized(nameof(Res.CmdDisableDailyUpdateCheck), typeof(Res))]
		DisableDailyUpdateCheck, // disables daily update check

		[DisplayNameLocalized(nameof(Res.CmdOpenDownloadWebsite), typeof(Res))]
		OpenDownloads, // Opens the download page

		[DisplayNameLocalized(nameof(Res.CmdOpenDocuWebsite), typeof(Res))]
		OpenDocu, // Opens the docu page

		[DisplayNameLocalized(nameof(Res.CmdOpenDiscordInvitation), typeof(Res))]
		OpenDiscordInvite, // Opens the Dicord invitation

		[DisplayNameLocalized(nameof(Res.CmdOpenForumWebsite), typeof(Res))]
		OpenForum, // Opens the Dicord invitation

		[DisplayNameLocalized(nameof(Res.CmdOpenProjectWebsite), typeof(Res))]
		OpenProject, // Opens the project website
	}
	
	public enum AlternateCommands
	{
		Guide = Commands.SrmGuide,
		Manual = Commands.SrmGuide,
		Srm = Commands.SrmGuide,
		Savestate = Commands.SavestateGuide,
		OC = Commands.OverwriteComp,
		Cmds = Commands.Help,
		CmdLine = Commands.Config,
		Cfg = Commands.Config,
		Q = Commands.Quit,
		B = Commands.Backup,
		BC = Commands.BackupComp,
		R = Commands.Restore,
		RC = Commands.RestoreComp,
		SBC = Commands.SlotByteComp,
		NSC = Commands.NonSlotComp,
		CS = Commands.ChecksumStatus,
		T = Commands.Transfer,
		Cls = Commands.Clear,
		L = Commands.Lang,
		CL = Commands.CompLang,
		C = Commands.Compare,
		O = Commands.Offset,
		EO = Commands.EditOffset,
		Load = Commands.LoadConfig,
		Save = Commands.SaveConfig,
		Open = Commands.OpenConfig,
		Log = Commands.OpenLog,
		LoadOn = Commands.AutoLoadOn,
		LoadOff = Commands.AutoLoadOff,
		SaveOn = Commands.AutoSaveOn,
		SaveOff = Commands.AutoSaveOff,
		Watch = Commands.WatchFile,
		Unwatch = Commands.UnwatchFile,
		EC = Commands.ExportCompResult,
		ECO = Commands.ExportCompResultOpen,
		ECS = Commands.ExportCompResultSelect,
		EF = Commands.ExportFlags,
		WF = Commands.FileWatchFlags,
		CF = Commands.ComparisonFlags,
		LF = Commands.LogFlags,
		W = Commands.WatchFile,
		UW = Commands.UnwatchFile,
		SS = Commands.SetSlot,
		SCS = Commands.SetCompSlot,
		Slot = Commands.ShowSlotSummary,
		ExportSlot = Commands.ExportSlotSummary,
		ES = Commands.ExportSlotSummary,
		Discord = Commands.OpenDiscordInvite,
		Forum = Commands.OpenForum,
		Project = Commands.OpenProject,
		Docu = Commands.OpenDocu,
		Downloads = Commands.OpenDownloads,
		Update = Commands.CheckForUpdate,
		UpdateNP = Commands.Update,
		UpdateOn = Commands.EnableDailyUpdateCheck,
		UpdateOff = Commands.DisableDailyUpdateCheck,
	}
}
