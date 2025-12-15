using System;
using System.Collections.Generic;

namespace SeiaStatusSystem.Core
{
    internal readonly struct Subscription<TStatusType> : IDisposable
    {
        public TargetToken TargetToken { get; init; }
        public TStatusType StatusType { get; init; }

        readonly Dictionary<int, Action<float>> subscriptions;
        readonly ObjectPool<Dictionary<int, Action<float>>> subscriptionsPool;

        public bool IsEmpty => subscriptions.Count == 0;

        public Subscription(TargetToken targetToken, TStatusType statusType, ObjectPool<Dictionary<int, Action<float>>> pool)
        {
            TargetToken = targetToken;
            StatusType = statusType;
            subscriptionsPool = pool;
            subscriptions = subscriptionsPool.Get();
        }

        public readonly void AddSubscription(int subscriptionActionId, Action<float> onStatusChanged)
        {
            subscriptions.Add(subscriptionActionId, onStatusChanged);
        }

        public readonly void RemoveSubscription(int subscriptionActionId)
        {
            subscriptions.Remove(subscriptionActionId);
        }

        public void Invoke(float value)
        {
            foreach (var onStatusChanged in subscriptions.Values)
            {
                onStatusChanged.Invoke(value);
            }
        }

        public readonly void Dispose()
        {
            subscriptionsPool.Release(subscriptions);
        }
    }
}