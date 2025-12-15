
using System;

namespace SeiaStatusSystem.Core
{
    internal class Disposable : IDisposable
    {
        public static readonly Disposable Empty = new(null);

        readonly Action? onDispose;
        bool disposed = false;

        public Disposable(Action? onDispose)
        {
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            if (disposed) return;

            onDispose?.Invoke();
            disposed = true;
        }
    }
}