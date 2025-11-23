using System;
using System.Threading;
using SkiaSharp;

namespace Janphe.Fantasy.Map
{
    /// <summary>
    /// Helper class to run MapJobs outside of the Godot scene tree and expose a
    /// synchronous rendering API for desktop utilities.
    /// </summary>
    public sealed class MapPreview : IDisposable
    {
        private readonly MapJobs _mapJobs;
        private readonly ManualResetEventSlim _waiter = new(false);
        private bool _disposed;

        public MapPreview(int width, int height, int? seed = null)
        {
            _mapJobs = new MapJobs();
            _mapJobs.Options.Width = width;
            _mapJobs.Options.Height = height;
            if (seed.HasValue)
            {
                _mapJobs.Options.MapSeed = seed.Value;
            }
        }

        /// <summary>
        /// Generates a new map bitmap.
        /// </summary>
        /// <param name="regenerate">Set to true to force map data recalculation.</param>
        /// <returns>Copy of the current map bitmap.</returns>
        public SKBitmap Render(bool regenerate = true)
        {
            EnsureNotDisposed();

            if (regenerate)
            {
                _mapJobs.Options.NeedUpdate = true;
            }

            _waiter.Reset();
            SKBitmap? copy = null;
            Exception? error = null;

            _mapJobs.processAsync(_ =>
            {
                try
                {
                    var bitmap = _mapJobs.Bitmap ?? throw new InvalidOperationException("Map was not rendered.");
                    copy = new SKBitmap(bitmap.Info);
                    bitmap.CopyTo(copy);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    _waiter.Set();
                }
            });

            _waiter.Wait();
            if (error != null)
            {
                throw error;
            }

            return copy ?? throw new InvalidOperationException("No bitmap was produced.");
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MapPreview));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _mapJobs.Bitmap?.Dispose();
            _waiter.Dispose();
            _disposed = true;
        }
    }
}
