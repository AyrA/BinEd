namespace BinEd
{
    internal class BinFile : IDisposable
    {
        private readonly FileStream source;
        private readonly string filename;

        public bool Disposed { get; private set; }
        public bool Readonly { get; private set; }
        public string FileName => filename;
        public bool EOF => source.Position == source.Length;
        public long Position { get => source.Position; set => source.Position = value; }

        public BinFile(string fileName, bool create)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or whitespace.", nameof(fileName));
            }

            filename = Path.GetFullPath(fileName);
            if (create)
            {
                source = File.Open(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                var fi = new FileInfo(fileName);
                if (fi.Attributes.HasFlag(FileAttributes.Hidden) || fi.Attributes.HasFlag(FileAttributes.System))
                {
                    throw new IOException($"Refusing to open '{fileName}' because it's marked as hidden or system. To edit this file, remove the offending attributes");
                }
                Readonly = fi.Attributes.HasFlag(FileAttributes.ReadOnly);
                source = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
        }

        public void Truncate(long? size)
        {
            var pos = size ?? source.Position;
            ArgumentOutOfRangeException.ThrowIfNegative(pos, nameof(size));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos, source.Position, nameof(size));
            EnsureWritable();
            source.SetLength(pos);
        }

        public void Insert(byte[] bytes)
        {
            if (EOF)
            {
                source.Write(bytes);
            }
            else
            {
                using var scratch = new ScratchFile();
                using (new PosRestore(source))
                {
                    source.CopyTo(scratch);
                }
                scratch.Flush();
                scratch.Position = 0;
                source.Write(bytes);
                scratch.CopyTo(source);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            source.Dispose();
            Disposed = true;
        }

        private void EnsureReady()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
        }

        private void EnsureWritable()
        {
            EnsureReady();
            if (Readonly)
            {
                throw new IOException("File is currently readonly");
            }
        }
    }
}
