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

        void Compare<TComparer>(IOptions options)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();
        void Compare<TComparer>(IOptions options, TextWriter output)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();

        void Compare<TComparer>(Stream currStream, Stream compStream, IOptions options, TextWriter output)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();

        void ExportComparison<TComparer>(IOptions options, bool showInExplorer = false)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();
        void ExportComparison<TComparer>(IOptions options, string filepath, bool showInExplorer = false)
            where TComparer : ISramComparer<TSramFile, TSramGame>, new();
    }
}