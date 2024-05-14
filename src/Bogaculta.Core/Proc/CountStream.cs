using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bogaculta.Proc
{
    [ObservableObject]
    public sealed partial class CountStream : Stream
    {
        private readonly Stream _real;

        internal CountStream(Stream real, Stream ctx)
        {
            _real = real;

            if (_real is FileStream fs)
            {
                FileName = fs.Name;
                FileSize = fs.Length;
            }

            if (ctx is { } cs)
            {
                FileSize = Math.Max(FileSize, cs.Length);
            }
        }

        [ObservableProperty] private string _fileName;
        [ObservableProperty] private long _fileSize;
        private long _readBytes;
        private long _writeBytes;
        [ObservableProperty] private double _readAmount;
        [ObservableProperty] private double _writeAmount;

        private double CalcPercent(long value)
        {
            return value / (_fileSize * 1.0);
        }

        private long ReadBytes
        {
            get => _readBytes;
            set => ReadAmount = CalcPercent(_readBytes = value);
        }

        private long WriteBytes
        {
            get => _writeBytes;
            set => WriteAmount = CalcPercent(_writeBytes = value);
        }

        public override void Flush()
        {
            _real.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = _real.Read(buffer, offset, count);
            ReadBytes += readBytes;
            return readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _real.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _real.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _real.Write(buffer, offset, count);
            WriteBytes += count;
        }

        public override bool CanRead => _real.CanRead;

        public override bool CanSeek => _real.CanSeek;

        public override bool CanWrite => _real.CanWrite;

        public override long Length => _real.Length;

        public override long Position
        {
            get => _real.Position;
            set => _real.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            _real.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            return _real.DisposeAsync();
        }
    }
}