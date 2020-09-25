using System;
using System.IO;
using SramCommons.Models;
using SramComparer.Enums;

namespace SramComparer.Services
{
    public interface ICommandHandler
    {
        bool RunCommand(string command, IOptions options, TextWriter? outStream = null);
    }

    public interface ICommandHandler<out TSramFile, out TSramGame> : ICommandHandler
        where TSramFile : SramFile, ISramFile<TSramGame>
        where TSramGame : struct
    {
        int GetGameId(int maxGameId);

        void OverwriteComparisonFileWithCurrentFile(IOptions options);

        void BackupSramFile(IOptions options, SramFileKind file, bool restore = false);

        Enum InvertIncludeFlag(Enum flags, Enum flag);

        void CompareFiles<TComparer>(IOptions options)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();

        void ExportCurrentComparison<TComparer>(IOptions options)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();
    }
}