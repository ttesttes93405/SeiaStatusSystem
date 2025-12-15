using System;

namespace SeiaStatusSystem.Core
{
    static class Utilities
    {

        static int nextId = 0;
        public static int GenerateUniqueId()
        {
            return System.Threading.Interlocked.Increment(ref nextId);
        }

        public static IDisposable CreateDisposable(Action onDispose)
        {
            return new Disposable(onDispose);
        }
        
        public static IDisposable CreateDisposable(params IDisposable[] disposables)
        {
            return new Disposable(() =>
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
            });
        }

    }


}