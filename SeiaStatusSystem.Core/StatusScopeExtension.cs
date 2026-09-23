
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


        public static IDisposable SubscribeEffect<TStatusType, TStatusInfo>(
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
                if (effectSubs.TryGetValue(statusEntityToken, out var existingSubscription))
                {
                    effectSubs[statusEntityToken] = existingSubscription + ApplyEffect;
                }
                else
                {
                    effectSubs[statusEntityToken] = ApplyEffect;
                }
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
                var effectSubs = statusScope.StatusEntityEffectSubscriptions;
                if (effectSubs.TryGetValue(statusEntityToken, out var existingSubscription))
                {
                    var remainingSubscriptions = existingSubscription - ApplyEffect;
                    if (remainingSubscriptions == null)
                    {
                        effectSubs.Remove(statusEntityToken);
                    }
                    else
                    {
                        effectSubs[statusEntityToken] = remainingSubscriptions;
                    }
                }

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

        [Obsolete("Use SubscribeEffect instead.")]
        public static IDisposable SuscribeEffect<TStatusType, TStatusInfo>(
            this StatusScope<TStatusType, TStatusInfo> statusScope,
            StatusEntityToken statusEntityToken,
            Func<Action> statusEffect
        )
            where TStatusType : Enum
            where TStatusInfo : IStatusInfo<TStatusType>
        {
            return SubscribeEffect(statusScope, statusEntityToken, statusEffect);
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

            var targetToken = modifier.TargetToken;

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, (v) => modifier.CalculateValue(v), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                modifier.CalculateValue(statusScope.GetStatusValue(targetToken, modifier.StatusType1));
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

            var targetToken = modifier.TargetToken;

            void Recalculate() => modifier.CalculateValue(
                statusScope.GetStatusValue(targetToken, modifier.StatusType1),
                statusScope.GetStatusValue(targetToken, modifier.StatusType2));

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, _ => Recalculate(), executeAfterSubscribe: false);
            var handler2 = Subscribe(statusScope, targetToken, modifier.StatusType2, _ => Recalculate(), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                Recalculate();
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

            var targetToken = modifier.TargetToken;

            void Recalculate() => modifier.CalculateValue(
                statusScope.GetStatusValue(targetToken, modifier.StatusType1),
                statusScope.GetStatusValue(targetToken, modifier.StatusType2),
                statusScope.GetStatusValue(targetToken, modifier.StatusType3));

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, _ => Recalculate(), executeAfterSubscribe: false);
            var handler2 = Subscribe(statusScope, targetToken, modifier.StatusType2, _ => Recalculate(), executeAfterSubscribe: false);
            var handler3 = Subscribe(statusScope, targetToken, modifier.StatusType3, _ => Recalculate(), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                Recalculate();
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

            var targetToken = modifier.TargetToken;

            void Recalculate() => modifier.CalculateValue(
                statusScope.GetStatusValue(targetToken, modifier.StatusType1),
                statusScope.GetStatusValue(targetToken, modifier.StatusType2),
                statusScope.GetStatusValue(targetToken, modifier.StatusType3),
                statusScope.GetStatusValue(targetToken, modifier.StatusType4));

            var handler1 = Subscribe(statusScope, targetToken, modifier.StatusType1, _ => Recalculate(), executeAfterSubscribe: false);
            var handler2 = Subscribe(statusScope, targetToken, modifier.StatusType2, _ => Recalculate(), executeAfterSubscribe: false);
            var handler3 = Subscribe(statusScope, targetToken, modifier.StatusType3, _ => Recalculate(), executeAfterSubscribe: false);
            var handler4 = Subscribe(statusScope, targetToken, modifier.StatusType4, _ => Recalculate(), executeAfterSubscribe: false);

            if (executeAfterSubscribe)
            {
                Recalculate();
            }

            return CreateDisposable(handler1, handler2, handler3, handler4);
        }
    }
}
