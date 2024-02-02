

using System.Diagnostics;

namespace BinEd
{
    internal class ScratchFile : Stream
    {
        private readonly FileStream fs;
        private readonly string filename;

        public ScratchFile()
        {
            filename = Path.GetTempFileName();
            fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
            File.Delete(filename);
            Debug.Print($"Created scratch file '{filename}'");
        }

        #region Overridden Props

        public override bool CanRead => fs.CanRead;

        public override bool CanSeek => fs.CanSeek;

        public override bool CanWrite => fs.CanWrite;

        public override long Length => fs.Length;

        public override long Position { get => fs.Position; set => fs.Position = value; }

        public override bool CanTimeout => fs.CanTimeout;

        public override int ReadTimeout { get => fs.ReadTimeout; set => fs.ReadTimeout = value; }

        public override int WriteTimeout { get => fs.WriteTimeout; set => fs.WriteTimeout = value; }

        #endregion

        #region Overriden Sync

        public override void Flush()
        {
            fs.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return fs.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return fs.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            fs.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            fs.Write(buffer, offset, count);
        }

        public override void Close()
        {
            fs.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            fs.CopyTo(destination, bufferSize);
        }

        protected override void Dispose(bool disposing)
        {
            fs.Dispose();
            base.Dispose(disposing);
            Debug.Print($"Dispose scratch file '{filename}'");
        }

        public override int Read(Span<byte> buffer)
        {
            return fs.Read(buffer);
        }

        public override int ReadByte()
        {
            return fs.ReadByte();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            fs.Write(buffer);
        }

        public override void WriteByte(byte value)
        {
            fs.WriteByte(value);
        }

        #endregion

        #region Overridden Async

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return fs.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return fs.BeginWrite(buffer, offset, count, callback, state);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return fs.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override async ValueTask DisposeAsync()
        {
            await fs.DisposeAsync();
            await base.DisposeAsync();
            Debug.Print($"Dispose scratch file '{filename}'");
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return fs.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            fs.EndWrite(asyncResult);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return fs.FlushAsync(cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return fs.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return fs.ReadAsync(buffer, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return fs.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return fs.WriteAsync(buffer, cancellationToken);
        }

        #endregion
    }
}
