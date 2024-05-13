using System.IO;
using System.Data;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bogaculta.Proc
{
    [ObservableObject]
    public sealed partial class CountStream : Stream
    {
        private readonly Stream _real;

        public CountStream(Stream real)
        {
            _real = real;

            if (_real is FileStream fs)
            {
                FileName = fs.Name;
                FileSize = fs.Length;
            }
        }

        [ObservableProperty] private string _fileName;
        [ObservableProperty] private long _fileSize;
        [ObservableProperty] private int _readBytes;
        [ObservableProperty] private int _writeBytes;

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