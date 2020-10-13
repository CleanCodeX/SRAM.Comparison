using System;

namespace SramComparer
{
	/// <summary>Common interface for comparing SRAM files</summary>
	public interface IOptions
	{
		/// <summary>Optional list of commands which should be performed in a row</summary>
		string? Commands { get; }

		/// <summary>Optional region of SRAM file. If Empty, United States is set.</summary>
		Enum Region { get; }

		/// <summary>The filepath to load the current SRAM file from</summary>
		string? CurrentGameFilepath { get; }

		/// <summary>The filepath to load the comparison SRAM file from. if <see cref="CurrentGameFilepath"/> is set, it will be automatically filled from that</summary>
		string? ComparisonGameFilepath { get; }

		/// <summary>If set, this directory will be used for exporting comparisons, otherwise the directory of <see cref="CurrentGameFilepath" /></summary>
		string? ExportDirectory { get; }

		/// <summary>Gets or sets if a specific game only of current game SRAM file should be compared. If  zero (default), all games will be compared.</summary>
		int CurrentGame { get; set; }

		/// <summary>Gets or sets if a specific game only of comparison game SRAM file should be compared. If zero (default), same game(s) as current game(s) will be compared.</summary>
		int ComparisonGame { get; set; }

		/// <summary>Optional game specific flags for comparisons</summary>
		Enum Flags { get; set; }
	}
}