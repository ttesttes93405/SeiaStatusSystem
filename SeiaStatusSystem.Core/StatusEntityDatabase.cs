using System;
using System.Collections.Generic;

namespace SeiaStatusSystem.Core
{
    class StatusEntityDatabase<TStatusType, TStatusInfo> : IDisposable
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {

        readonly Dictionary<StatusEntityToken, StatusEntity<TStatusType>> statusByToken = new();
        readonly Dictionary<TargetToken, HashSet<StatusEntity<TStatusType>>> statusByTargetToken = new();
        readonly Dictionary<TargetTypeToken<TStatusType>, HashSet<StatusEntity<TStatusType>>> statusByTargetTypeToken = new();

        readonly ObjectPool<HashSet<StatusEntity<TStatusType>>> statusPool;

        public StatusEntityDatabase()
        {
            statusPool = new(
                createFunc: () => new HashSet<StatusEntity<TStatusType>>(),
                releaseFunc: list => list.Clear()
            );
        }

        public void Add(StatusEntity<TStatusType> statusEntity)
        {
            statusByToken.Add(statusEntity.Token, statusEntity);

            var targetToken = statusEntity.TargetToken;
            var statusType = statusEntity.Info.Type;
            var key = new TargetTypeToken<TStatusType>(targetToken, statusType);

            AddItem(statusByTargetToken, targetToken, statusEntity, statusPool);
            AddItem(statusByTargetTypeToken, key, statusEntity, statusPool);

            static void AddItem<K, T, L>(Dictionary<K, L> dict, K key, T item, ObjectPool<L> pool) where L : class, ICollection<T>
            {
                if (dict.TryGetValue(key, out var list) == false)
                {
                    list = pool.Get();
                    dict[key] = list;
                }
                list.Add(item);
            }
        }

        public void Remove(StatusEntityToken statusEntityToken)
        {
            if (statusByToken.TryGetValue(statusEntityToken, out var statusEntity) == false)
                return;

            statusByToken.Remove(statusEntityToken);

            var targetToken = statusEntity.TargetToken;
            var statusType = statusEntity.Info.Type;
            var key = new TargetTypeToken<TStatusType>(targetToken, statusType);


            RemoveItem(statusByTargetToken, targetToken, statusEntity, statusPool);
            RemoveItem(statusByTargetTypeToken, key, statusEntity, statusPool);
            if (statusByTargetToken.TryGetValue(targetToken, out var statusEntitiesByTargetToken) == true)
            {
                statusEntitiesByTargetToken.Remove(statusEntity);
                if (statusEntitiesByTargetToken.Count == 0)
                {
                    statusByTargetToken.Remove(targetToken);
                    statusPool.Release(statusEntitiesByTargetToken);
                }
            }

            if (statusByTargetTypeToken.TryGetValue(key, out var statusEntitiesByTargetTypeToken) == true)
            {
                statusEntitiesByTargetTypeToken.Remove(statusEntity);
                if (statusEntitiesByTargetTypeToken.Count == 0)
                {
                    statusByTargetTypeToken.Remove(key);
                    statusPool.Release(statusEntitiesByTargetTypeToken);
                }
            }

            static void RemoveItem<K, T, L>(Dictionary<K, L> dict, K key, T item, ObjectPool<L> pool) where L : class, ICollection<T>
            {
                if (dict.TryGetValue(key, out var list) == true)
                {
                    list.Remove(item);
                    if (list.Count == 0)
                    {
                        dict.Remove(key);
                        pool.Release(list);
                    }
                }
            }
        }

        public bool Contains(StatusEntityToken statusEntityToken)
        {
            return statusByToken.ContainsKey(statusEntityToken);
        }

        public bool TryGetByEntityToken(StatusEntityToken statusEntityToken, out StatusEntity<TStatusType> statusEntity)
        {
            return statusByToken.TryGetValue(statusEntityToken, out statusEntity);
        }

        public bool TryGetByTargetToken(
            TargetToken targetToken,
            out HashSet<StatusEntity<TStatusType>> statusEntities
        )
        {
            return statusByTargetToken.TryGetValue(targetToken, out statusEntities);
        }

        public bool TryGetByTargetTypeToken(
            TargetTypeToken<TStatusType> key,
            out HashSet<StatusEntity<TStatusType>> statusEntities
        )
        {
            return statusByTargetTypeToken.TryGetValue(key, out statusEntities);
        }

        public void Dispose()
        {
            statusByToken.Clear();
            statusByTargetToken.Clear();
            statusByTargetTypeToken.Clear();
            statusPool.Dispose();
        }
    }
}