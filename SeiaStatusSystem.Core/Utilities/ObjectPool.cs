using System;
using System.Collections.Generic;

namespace SeiaStatusSystem.Core
{
    public class ObjectPool<T> : IDisposable where T : class
    {
        readonly Queue<T> queue;
        readonly Func<T> createFunc;
        readonly Action<T>? getFunc;
        readonly Action<T>? releaseFunc;
        private readonly int maxSize;


        public int CountCreated { get; private set; }
        public int CountInPool => queue.Count;

        public ObjectPool(Func<T> createFunc, Action<T>? getFunc = null, Action<T>? releaseFunc = null, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFunc == null)
            {
                throw new ArgumentNullException("createFunc");
            }

            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }

            queue = new Queue<T>(defaultCapacity);

            this.createFunc = createFunc;
            this.maxSize = maxSize;
            this.getFunc = getFunc;
            this.releaseFunc = releaseFunc;
        }

        public T Get()
        {
            T val;
            if (queue.Count == 0)
            {
                val = createFunc();
                CountCreated++;
            }
            else
            {
                val = queue.Dequeue();
            }

            getFunc?.Invoke(val);
            return val;
        }

        public void Release(T element)
        {
            if (queue.Count > 0)
            {
                if (queue.Contains(element))
                {
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }

            releaseFunc?.Invoke(element);
            if (CountInPool < maxSize)
            {
                queue.Enqueue(element);
                return;
            }

            CountCreated--;
        }

        public void Clear()
        {
            queue.Clear();
            CountCreated = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}