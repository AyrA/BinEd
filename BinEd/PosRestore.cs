namespace BinEd
{
    /// <summary>
    /// Restores stream position to initial value when disposed
    /// </summary>
    /// <param name="stream">stream</param>
    internal class PosRestore : IDisposable
    {
        private readonly object _lock = new();
        private readonly Stream _stream;

        private bool disposed;

        public long Position { get; }

        public PosRestore(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream not seekable");
            }

            _stream = stream;
            Position = stream.Position;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                //Don't restore the position more than once.
                //Prevents multiple "using" or .Dispose() calls from messing up the position
                if (disposed)
                {
                    return;
                }
                try
                {
                    _stream.Position = Position;
                    disposed = true;
                }
                catch
                {
                    //NOOP
                }
            }
        }
    }
}
