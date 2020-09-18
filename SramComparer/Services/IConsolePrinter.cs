using System;

namespace SramComparer.Services
{
    public interface IConsolePrinter
    {
        void PrintSectionHeader();
        
        void PrintBufferInfo(string bufferName, int bufferOffset, int bufferLength);

        void PrintComparison(string ident, int offset, string? offsetName, ushort currValue, ushort compValue);
        void PrintComparison(string ident, int offset, string? offsetName, byte currValue, byte compValue);

        void PrintError(Exception ex);
        void PrintError(string message);
        void PrintFatalError(string fataError);

        void PrintInvertIncludeFlag(Enum flags, Enum flag);

        void PrintManual();
        void PrintCommands();
        void PrintSettings(IOptions options);
        
    }
}