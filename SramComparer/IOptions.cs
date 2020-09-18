using System;

namespace SramComparer
{
    public interface IOptions
    {
        string? Commands { get; }
        string CurrentGameFilepath { get; }
        string ComparisonGameFilepath { get; }
        string ExportDirectory { get; }
        int Game { get; set; }
        int ComparisonGame { get; set; }
        Enum Region { get; }
        Enum Flags { get; set; }
    }
}