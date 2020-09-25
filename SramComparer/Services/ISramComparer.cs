using SramCommons.Models;

namespace SramComparer.Services
{
    public interface ISramComparer<in TSramFile, in TSramGame>
        where TSramFile : SramFile, ISramFile<TSramGame>
        where TSramGame : struct
    {
        int CompareSram(TSramFile currFile, TSramFile compFile, IOptions options);
        int CompareGame(TSramGame currGame, TSramGame compGame, IOptions options);
    }
}