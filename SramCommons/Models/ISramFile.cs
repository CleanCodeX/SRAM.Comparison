using System;

namespace SramCommons.Models
{
    public interface ISramFile
    {
        bool Save(string filename);
        public bool IsValid();
        Span<byte> GetCurrentGameBytes();
    }

    public interface ISramFile<out TSramGame, in TGameId> : ISramFile
        where TGameId: struct, Enum
        where TSramGame : struct
    {
        TSramGame GetGame(TGameId gameId);
        
        public bool IsValid(TGameId gameId);
    }
}