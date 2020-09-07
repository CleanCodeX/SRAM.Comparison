using System;

namespace SramComparer
{
    public interface IOptions
    {
        string CurrentGameFilepath { get; }
        string ComparisonGameFilepath { get; }
        string ExportDirectory { get; }
        Enum Region { get; }
        Enum Game { get; }
        Enum ComparisonGame { get; }
        Enum Flags { get; }
    }

    public abstract class OptionsBase<TFileRegion, TGameId, TComparisonFlags> : IOptions
        where TFileRegion : struct, Enum
        where TGameId: struct, Enum
        where TComparisonFlags : struct, Enum
    {
#nullable disable
        public string CurrentGameFilepath { get; set; }
        public string ComparisonGameFilepath { get; set; }
        public string ExportDirectory { get; set; }
#nullable restore

        public TFileRegion Region { get; set; }
        public TGameId Game { get; set; }
        public TGameId ComparisonGame { get; set; }
        public TComparisonFlags Flags;

        Enum IOptions.Region => Region;
        Enum IOptions.Game => Game;
        Enum IOptions.ComparisonGame => ComparisonGame;
        Enum IOptions.Flags => Flags;
    }
}