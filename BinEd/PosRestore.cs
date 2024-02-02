using System.Diagnostics;

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

        /// <summary>
        /// Gets the position that the stream is restored to
        /// when this instance is disposed
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Gets or sets whether the position should be restored or not.
        /// If set to false, the position is not restored when this instance is disposed
        /// </summary>
        /// <remarks>Default is true</remarks>
        public bool Restore { get; set; }

        public PosRestore(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream not seekable");
            }

            _stream = stream;
            Position = stream.Position;
            Restore = true;
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
                Debug.Print("Disposing {0}", nameof(PosRestore));
                if (Restore)
                {
                    try
                    {
                        _stream.Position = Position;
                    }
                    catch
                    {
                        Debug.Print("Failed to restore position to {0}", Position);
                        //NOOP
                    }
                }
                else
                {
                    Debug.Print("User disabled stream restore. Will not restore position to {0}", Position);
                }
                disposed = true;
            }
        }
    }
}
