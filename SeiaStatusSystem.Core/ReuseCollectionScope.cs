using System;
using System.Collections.Generic;

namespace SeiaStatusSystem.Core
{
    readonly ref struct ReuseCollectionScope<T>
    {
        readonly ICollection<T> collection;
        public ICollection<T> Value => collection;
        public ReuseCollectionScope(ICollection<T> collection)
        {
            this.collection = collection;
            if (collection.Count > 0)
            {
                collection.Clear();
            }
        }

        public readonly void Dispose()
        {
            collection.Clear();
        }
    }
}