namespace BinEd
{
    internal class BinFile : IDisposable
    {
        private readonly FileStream data;
        private readonly string filename;

        public bool Disposed { get; private set; }
        public bool Readonly { get; private set; }
        public string FileName => filename;

        public BinFile(string fileName, bool create)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or whitespace.", nameof(fileName));
            }

            filename = Path.GetFullPath(fileName);
            if (create)
            {
                data = File.Open(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                var fi = new FileInfo(fileName);
                if (fi.Attributes.HasFlag(FileAttributes.Hidden) || fi.Attributes.HasFlag(FileAttributes.System))
                {
                    throw new IOException($"Refusing to open '{fileName}' because it's marked as hidden or system. To edit this file, remove the offending attributes");
                }
                Readonly = fi.Attributes.HasFlag(FileAttributes.ReadOnly);
                data = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
        }

        public void Truncate(long? size)
        {
            var pos = size ?? data.Position;
            ArgumentOutOfRangeException.ThrowIfNegative(pos, nameof(size));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos, data.Position, nameof(size));
            EnsureWritable();
            data.SetLength(pos);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            data.Dispose();
            Disposed = true;
        }

        private void EnsureWritable()
        {
            if (Readonly)
            {
                throw new IOException("File is currently readonly");
            }
        }
    }
}
