using System;

namespace SramCommons.Models
{
    public interface ISramFile<out TSramGame, in TGameId> : ISramFile
        where TGameId: struct, Enum
        where TSramGame : struct
    {
        TSramGame GetGame(TGameId gameId);
        
        public bool IsValid(TGameId gameId);
    }

    public interface ISramFile
    {
        bool Save(string filename);
        public bool IsValid();
        Span<byte> GetCurrentGameBytes();
    }
}