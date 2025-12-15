
using System;
using static SeiaStatusSystem.Core.Utilities;

namespace SeiaStatusSystem.Core
{
    public static class StatusScopeExtension
    {
        public static SubscriptionHandler<TStatusType> Subscribe<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            TargetToken targetToken,
            TStatusType statusType,
            Action<float> onStatusChanged,
            bool executeAfterSubscribe = true
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            statusScope.ThrowIfDisposed();

            var subscriptions = statusScope.targetTypeSubscriptions;
            var subscriptionsPool = statusScope.subscriptionsPool;
            var disposables = statusScope.disposables;

            var key = new TargetTypeToken<TStatusType>(targetToken, statusType);

            if (subscriptions.TryGetValue(key, out var subscription) == false)
            {
                subscription = new Subscription<TStatusType>(targetToken, statusType, subscriptionsPool);
                subscriptions[key] = subscription;
                disposables.Add(subscription);
            }

            var handler = new SubscriptionHandler<TStatusType>(subscription, GenerateUniqueId(), onStatusChanged);
            disposables.Add(handler);

            if (executeAfterSubscribe)
            {
                statusScope.UpdateSubscriptionValue(key, onStatusChanged);
            }

            return handler;
        }


        public static IDisposable SuscribeEffect<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            StatusEntityToken statusEntityToken,
            Func<Action> statusEffect
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            statusScope.ThrowIfDisposed();

            var isAlive = statusScope.IsEntityAlive(statusEntityToken);
            var isPending = statusScope.IsEntityPending(statusEntityToken);

            if (isAlive == false && isPending == false)
            {
                // maybe log warning here?
                return Disposable.Empty;
            }

            Action? cleaner = null;
            var effectCleaners = statusScope.StatusEntityEffectCleaners;

            if (isPending == true)
            {
                var effectSubs = statusScope.StatusEntityEffectSubscriptions;
                effectSubs[statusEntityToken] = ApplyEffect;
            }
            else
            {
                ApplyEffect();
            }


            return new Disposable(CreateDisposer);

            void ApplyEffect()
            {
                cleaner = statusEffect();
                if (effectCleaners.TryGetValue(statusEntityToken, out var existingCleaner))
                {
                    existingCleaner += cleaner;
                    effectCleaners[statusEntityToken] = existingCleaner;
                    return;
                }
                else
                {
                    effectCleaners[statusEntityToken] = cleaner!;
                }
            }


            void CreateDisposer()
            {
                if (effectCleaners.TryGetValue(statusEntityToken, out var existingCleaner))
                {
                    existingCleaner -= cleaner;
                    if (existingCleaner == null)
                    {
                        effectCleaners.Remove(statusEntityToken);
                    }
                    else
                    {
                        effectCleaners[statusEntityToken] = existingCleaner;
                    }
                }
            }
        }






        public static IDisposable Subscribe<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            ModifierT1<TStatusType, TStatusInfo> modifier,
            bool executeAfterSubscribe = true
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            statusScope.ThrowIfDisposed();

            float v = modifier.Get(statusScope, out var value);
            float v1 = value;

            var targetToken = modifier.TargetToken;

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, (v) => modifier.CalculateValue(v), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                modifier.CalculateValue(v1);
            }

            return CreateDisposable(handler1);

        }

        public static IDisposable Subscribe<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            ModifierT2<TStatusType, TStatusInfo> modifier,
            bool executeAfterSubscribe = true
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            statusScope.ThrowIfDisposed();

            float v = modifier.Get(statusScope, out var values);
            (float v1, float v2) = values;

            var targetToken = modifier.TargetToken;

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, v => modifier.CalculateValue(v, v2), executeAfterSubscribe: false);
            var handler2 = Subscribe(statusScope, targetToken, modifier.StatusType2, v => modifier.CalculateValue(v1, v), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                modifier.CalculateValue(v1, v2);
            }

            return CreateDisposable(handler1, handler2);
        }

        public static IDisposable Subscribe<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            ModifierT3<TStatusType, TStatusInfo> modifier,
            bool executeAfterSubscribe = true
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            statusScope.ThrowIfDisposed();

            float v = modifier.Get(statusScope, out var values);
            (float v1, float v2, float v3) = values;

            var targetToken = modifier.TargetToken;

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, v => modifier.CalculateValue(v, v2, v3), executeAfterSubscribe: false);
            var handler2 = Subscribe(statusScope, targetToken, modifier.StatusType2, v => modifier.CalculateValue(v1, v, v3), executeAfterSubscribe: false);
            var handler3 = Subscribe(statusScope, targetToken, modifier.StatusType3, v => modifier.CalculateValue(v1, v2, v), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                modifier.CalculateValue(v1, v2, v3);
            }

            return CreateDisposable(handler1, handler2, handler3);
        }

        public static IDisposable Subscribe<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            ModifierT4<TStatusType, TStatusInfo> modifier,
            bool executeAfterSubscribe = true
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            statusScope.ThrowIfDisposed();

            float v = modifier.Get(statusScope, out var values);
            (float v1, float v2, float v3, float v4) = values;

            var targetToken = modifier.TargetToken;

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, v => modifier.CalculateValue(v, v2, v3, v4), executeAfterSubscribe: false);
            var handler2 = Subscribe(statusScope, targetToken, modifier.StatusType2, v => modifier.CalculateValue(v1, v, v3, v4), executeAfterSubscribe: false);
            var handler3 = Subscribe(statusScope, targetToken, modifier.StatusType3, v => modifier.CalculateValue(v1, v2, v, v4), executeAfterSubscribe: false);
            var handler4 = Subscribe(statusScope, targetToken, modifier.StatusType4, v => modifier.CalculateValue(v1, v2, v3, v), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                modifier.CalculateValue(v1, v2, v3, v4);
            }

            return CreateDisposable(handler1, handler2, handler3, handler4);
        }
    }
}