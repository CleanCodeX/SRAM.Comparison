using System;

namespace SramComparer
{
    public interface IOptions
    {
        string? Commands { get; }
        string CurrentGameFilepath { get; }
        string ComparisonGameFilepath { get; }
        string ExportDirectory { get; }
        int Game { get; }
        int ComparisonGame { get; }
        Enum Region { get; }
        Enum Flags { get; }
    }
}