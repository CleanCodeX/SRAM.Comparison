using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using App.Commons.Extensions;
using SramCommons.Exceptions;
using SramCommons.Extensions;
using SramCommons.Helpers;

namespace SramCommons.Models
{
    public abstract class SramFileBase<TSram, TSramGame, TFileRegion, TGameId> : SramFileBase, ISramFile<TSramGame, TGameId>
        where TSram : struct
        where TSramGame : struct
        where TGameId : struct, Enum
        where TFileRegion : struct, Enum
    {
        private TSramGame _currentGame;

        protected int FirstGameOffset { get; }

        public ref TSramGame CurrentGame => ref _currentGame;
        public TSram Sram { get; }
        public TGameId CurrentGameId { get; protected set; }
        public TFileRegion GameRegion { get; set; }
        
        protected SramFileBase(string filename, TFileRegion region, int sramSize, int gameSize, int firstGameOffset) :base(sramSize, gameSize)
        {
            Debug.Assert(SramSize == Marshal.SizeOf<TSram>());
            Debug.Assert(gameSize == Marshal.SizeOf<TSramGame>());

            FirstGameOffset = firstGameOffset;
            GameRegion = region;
            SramBuffer = LoadSramBufferFromFile(filename);
            Sram = GetSramStructFromBuffer(SramBuffer);
        }

        public abstract TSramGame GetGame(TGameId gameId);

        public bool IsValid() => IsValid(CurrentGameId);
        public virtual bool IsValid(TGameId gameId) => gameId.ToInt() >= 1 && gameId.ToInt() <= gameId.GetMaxValue().ToInt();

        protected int ToIndex(TGameId gameId) => (int)(object)gameId - 1;

        public virtual Span<byte> GetCurrentGameBytes()
        {
            var startOffset = FirstGameOffset + CurrentGameId.ToIndex() * GameSize;
            var endOffset = startOffset + GameSize;

            return SramBuffer.AsSpan()[startOffset..endOffset];
        }

        private static TSram GetSramStructFromBuffer(byte[] buffer) => Serializer.Deserialize<TSram>(buffer);
    }

    public abstract class SramFileBase
    {
#nullable disable
        public byte[] SramBuffer { get; protected set; }
#nullable restore
        public bool IsModified { get; set; }

        protected int SramSize { get; }
        protected int GameSize { get; }

        protected SramFileBase(int sramSize, int gameSize) => (SramSize, GameSize) = (sramSize, gameSize);

        protected byte[] LoadSramBufferFromFile(string filename)
        {
            using var file = new FileStream(filename, FileMode.Open);

            if (file is null)
                throw new InvalidSramFileException(FileError.FileNotFound);

            file.Seek(0, SeekOrigin.End);

            if (file.Position != SramSize)
                throw new InvalidSramFileException(FileError.InvalidSize);

            var sram = new byte[SramSize];

            file.Seek(0, SeekOrigin.Begin);
            file.Read(sram, 0, SramSize);
            file.Close();

            return sram;
        }

        public virtual bool Save(string filename)
        {
            using var file = new FileStream(filename, FileMode.Create);

            file.Write(SramBuffer, 0, SramSize);

            if (file.Position != SramSize)
                return false;

            file.Close();
            IsModified = false;

            return true;
        }
    }
}