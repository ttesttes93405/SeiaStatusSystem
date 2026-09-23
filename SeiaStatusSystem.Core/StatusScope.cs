using System;
using System.Collections.Generic;
using static SeiaStatusSystem.Core.Utilities;

namespace SeiaStatusSystem.Core
{
    public class StatusScope<TStatusType, TStatusInfo> : IDisposable
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {

        internal readonly HashSet<IDisposable> disposables = new();

        readonly StatusEntityDatabase<TStatusType, TStatusInfo> statusEntityDatabase = new();

        readonly Dictionary<TargetTypeToken<TStatusType>, StatusValue> statusValues = new();
        readonly HashSet<StatusEntityToken> checkExpiredStatus = new();

        internal Dictionary<StatusEntityToken, Action> StatusEntityEffectSubscriptions { get; init; } = new();
        internal Dictionary<StatusEntityToken, Action> StatusEntityEffectCleaners { get; init; } = new();

        internal readonly Dictionary<TargetTypeToken<TStatusType>, Subscription<TStatusType>> targetTypeSubscriptions = new();
        internal readonly ObjectPool<Dictionary<int, Action<float>>> subscriptionsPool;

        readonly Queue<StatusEntity<TStatusType>> pendingApplyStatus = new();
        readonly HashSet<StatusEntityToken> pendingRemovalEntityTokens = new();




        TimeSpan CurrentTime { get; set; }
        public bool IsDisposed { get; private set; }

        internal StatusScope()
        {
            IsDisposed = false;
            CurrentTime = TimeSpan.Zero;

            subscriptionsPool = new(
                createFunc: () => new Dictionary<int, Action<float>>(),
                releaseFunc: dict => dict.Clear()
            );

            disposables.Add(statusEntityDatabase);
            disposables.Add(subscriptionsPool);
        }

        public void Update(int step)
        {
            Update(TimeSpan.FromTicks(step));
        }

        public void Update(TimeSpan time)
        {
            ThrowIfDisposed();

            UpdateTime(time);
            Dash();
        }

        public void UpdateTime(TimeSpan time)
        {
            ThrowIfDisposed();

            CurrentTime = time;
        }



        readonly ref struct CombineCollection<T>
        {
            public readonly ICollection<T> First;
            public readonly ICollection<T> Second;
            public CombineCollection(ICollection<T> first, ICollection<T> second)
            {
                First = first;
                Second = second;
            }

            public readonly bool TryNext(out T item)
            {
                if (First.Count > 0)
                {
                    using var enumerator = First.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        item = enumerator.Current;
                        return true;
                    }
                }

                if (Second.Count > 0)
                {
                    using var enumerator = Second.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        item = enumerator.Current;
                        return true;
                    }
                }

                item = default!;
                return false;
            }
        }

        readonly HashSet<StatusEntityToken> applyStatusEntityTokens = new();
        readonly HashSet<StatusEntityToken> removeStatusEntityTokens = new();
        readonly HashSet<TargetTypeToken<TStatusType>> modifiedTargetTypeTokens = new();
        public void Dash()
        {
            ThrowIfDisposed();

            using (var applies = new ReuseCollectionScope<StatusEntityToken>(applyStatusEntityTokens))
            using (var removes = new ReuseCollectionScope<StatusEntityToken>(removeStatusEntityTokens))
            using (var modifies = new ReuseCollectionScope<TargetTypeToken<TStatusType>>(modifiedTargetTypeTokens))
            {
                MarkExpiredStatus();

                ProcessPendingAdditions(applies.Value, removes.Value, modifies.Value);
                ProcessPendingRemovals(applies.Value, removes.Value, modifies.Value);

                CalculateModifiedStatusValue(modifies.Value);

                PublishStatusEffect(applies.Value, removes.Value);
                UpdateSubscriptionsAndPublish(modifies.Value);
            }


            void MarkExpiredStatus()
            {
                foreach (var statusEntityId in checkExpiredStatus)
                {
                    if (statusEntityDatabase.TryGetByEntityToken(statusEntityId, out var statusEntity) == false)
                        continue;

                    if (IsExpired(statusEntity, CurrentTime) == true)
                    {
                        pendingRemovalEntityTokens.Add(statusEntity.Token);
                    }
                }
            }

            void ProcessPendingAdditions(ICollection<StatusEntityToken> applyStatusEntityTokens, ICollection<StatusEntityToken> removeStatusEntityTokens, ICollection<TargetTypeToken<TStatusType>> modifiedTargetTypeTokens)
            {
                while (pendingApplyStatus.Count > 0)
                {
                    var statusEntity = pendingApplyStatus.Dequeue();

                    if (pendingRemovalEntityTokens.Remove(statusEntity.Token))
                    {
                        StatusEntityEffectSubscriptions.Remove(statusEntity.Token);
                        continue;
                    }

                    applyStatusEntityTokens.Add(statusEntity.Token);

                    var statusEntityToken = statusEntity.Token;
                    var targetToken = statusEntity.TargetToken;
                    var statusType = statusEntity.Info.Type;
                    var key = new TargetTypeToken<TStatusType>(targetToken, statusType);

                    modifiedTargetTypeTokens.Add(key);

                    if (statusValues.TryGetValue(key, out var group) == false)
                    {
                        group = new StatusValue();
                        statusValues[key] = group;
                    }

                    statusEntityDatabase.Add(statusEntity);

                    if (statusEntity.Info.Duration.HasValue)
                    {
                        checkExpiredStatus.Add(statusEntityToken);
                    }
                }


            }

            void ProcessPendingRemovals(ICollection<StatusEntityToken> applyStatusEntityTokens, ICollection<StatusEntityToken> removeStatusEntityTokens, ICollection<TargetTypeToken<TStatusType>> modifiedTargetTypeTokens)
            {
                foreach (var statusEntityToken in pendingRemovalEntityTokens)
                {
                    checkExpiredStatus.Remove(statusEntityToken);

                    if (statusEntityDatabase.TryGetByEntityToken(statusEntityToken, out var statusEntity) == true)
                    {
                        removeStatusEntityTokens.Add(statusEntityToken);
                        statusEntityDatabase.Remove(statusEntityToken);
                        var key = new TargetTypeToken<TStatusType>(statusEntity.TargetToken, statusEntity.Info.Type);
                        modifiedTargetTypeTokens.Add(key);
                    }
                }

                pendingRemovalEntityTokens.Clear();
            }


            void CalculateModifiedStatusValue(ICollection<TargetTypeToken<TStatusType>> modifiedTargetTypeTokens)
            {
                foreach (var key in modifiedTargetTypeTokens)
                {
                    if (statusEntityDatabase.TryGetByTargetTypeToken(key, out var entities) == false)
                    {
                        statusValues.Remove(key);
                        continue;
                    }

                    float totalValue = 0F;
                    foreach (var entity in entities)
                    {
                        totalValue += entity.Info.Value;
                    }
                    statusValues[key] = new StatusValue(totalValue, CurrentTime);
                }
            }

            void PublishStatusEffect(ICollection<StatusEntityToken> applyStatusEntityTokens, ICollection<StatusEntityToken> removeStatusEntityTokens)
            {
                foreach (var statusEntityToken in applyStatusEntityTokens)
                {
                    if (StatusEntityEffectSubscriptions.TryGetValue(statusEntityToken, out var v) == true)
                    {
                        StatusEntityEffectSubscriptions.Remove(statusEntityToken);
                        v?.Invoke();
                    }
                }

                foreach (var statusEntityToken in removeStatusEntityTokens)
                {
                    if (StatusEntityEffectCleaners.TryGetValue(statusEntityToken, out var v) == true)
                    {
                        StatusEntityEffectCleaners.Remove(statusEntityToken);
                        v?.Invoke();
                    }
                }
            }

            void UpdateSubscriptionsAndPublish(ICollection<TargetTypeToken<TStatusType>> modifiedTargetTypeTokens)
            {

                foreach (var key in modifiedTargetTypeTokens)
                {
                    if (targetTypeSubscriptions.TryGetValue(key, out var sub) == false)
                        continue;

                    if (sub.IsEmpty)
                    {
                        disposables.Remove(sub);
                        targetTypeSubscriptions.Remove(key);
                        sub.Dispose();
                        continue;
                    }

                    UpdateSubscriptionValue(key, sub.Invoke);
                }
            }


            static bool IsExpired(StatusEntity<TStatusType> statusEntity, TimeSpan currentTime)
            {
                if (statusEntity.Info.Duration.HasValue == false)
                    return false;

                var duration = statusEntity.Info.Duration.Value;
                return statusEntity.CreateTime + duration <= currentTime;
            }

        }

        public bool IsEntityPending(StatusEntityToken statusEntityToken)
        {
            ThrowIfDisposed();

            foreach (var statusEntity in pendingApplyStatus)
            {
                if (statusEntity.Token == statusEntityToken && pendingRemovalEntityTokens.Contains(statusEntityToken) == false)
                    return true;
            }
            return false;
        }

        public bool IsEntityAlive(StatusEntityToken statusEntityToken)
        {
            ThrowIfDisposed();

            return statusEntityDatabase.Contains(statusEntityToken);
        }



        public StatusEntityToken Apply(TargetToken targetToken, TStatusInfo info)
        {
            ThrowIfDisposed();

            var statusEntity = new StatusEntity<TStatusType>(GenerateUniqueId(), CurrentTime, info, targetToken);

            pendingApplyStatus.Enqueue(statusEntity);

            return statusEntity.Token;
        }

        public void RemoveByTag(TargetToken targetToken, Tag tag)
        {
            ThrowIfDisposed();

            if (tag == Tag.None)
            {
                throw new ArgumentException("Tag.None cannot be used to remove statuses by tag.", nameof(tag));
            }

            if (statusEntityDatabase.TryGetByTargetToken(targetToken, out var statusEntities) == true)
            {
                foreach (var statusEntity in statusEntities)
                {
                    if (statusEntity.Info.Tag == tag)
                    {
                        pendingRemovalEntityTokens.Add(statusEntity.Token);
                    }
                }
            }

            foreach (var statusEntity in pendingApplyStatus)
            {
                if (statusEntity.TargetToken == targetToken && statusEntity.Info.Tag == tag)
                {
                    pendingRemovalEntityTokens.Add(statusEntity.Token);
                }
            }
        }

        public void RemoveByEntityToken(StatusEntityToken entityToken)
        {
            ThrowIfDisposed();

            pendingRemovalEntityTokens.Add(entityToken);

        }

        public void CleanTarget(TargetToken targetToken)
        {
            ThrowIfDisposed();

            if (statusEntityDatabase.TryGetByTargetToken(targetToken, out var statusEntities) == true)
            {
                foreach (var statusEntity in statusEntities)
                {
                    pendingRemovalEntityTokens.Add(statusEntity.Token);
                }
            }

            foreach (var statusEntity in pendingApplyStatus)
            {
                if (statusEntity.TargetToken == targetToken)
                {
                    pendingRemovalEntityTokens.Add(statusEntity.Token);
                }
            }
        }

        public float GetStatusValue(TargetToken targetToken, TStatusType statusType)
        {
            ThrowIfDisposed();

            var key = new TargetTypeToken<TStatusType>(targetToken, statusType);

            if (statusValues.TryGetValue(key, out var v) == true)
            {
                return v.TotalValue;
            }
            else
            {
                return 0F;
            }
        }


        internal void UpdateSubscriptionValue(TargetTypeToken<TStatusType> key, Action<float> onStatusChanged)
        {
            var value = GetStatusValue(key.TargetToken, key.StatusType);
            onStatusChanged(value);
        }


        internal void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(StatusScope<TStatusType, TStatusInfo>));
        }


        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            var cleaners = new List<Action>();
            foreach (var cleaner in StatusEntityEffectCleaners.Values)
            {
                if (cleaner == null)
                    continue;

                foreach (Action action in cleaner.GetInvocationList())
                    cleaners.Add(action);
            }
            StatusEntityEffectCleaners.Clear();
            StatusEntityEffectSubscriptions.Clear();

            List<Exception>? exceptions = null;
            foreach (var cleaner in cleaners)
            {
                try
                {
                    cleaner();
                }
                catch (Exception exception)
                {
                    (exceptions ??= new List<Exception>()).Add(exception);
                }
            }

            pendingApplyStatus.Clear();
            pendingRemovalEntityTokens.Clear();
            checkExpiredStatus.Clear();
            statusValues.Clear();
            targetTypeSubscriptions.Clear();

            foreach (var d in disposables)
            {
                try
                {
                    d.Dispose();
                }
                catch (Exception exception)
                {
                    (exceptions ??= new List<Exception>()).Add(exception);
                }
            }
            disposables.Clear();

            if (exceptions != null)
                throw new AggregateException("One or more status scope resources could not be cleaned up.", exceptions);
        }

    }

}
