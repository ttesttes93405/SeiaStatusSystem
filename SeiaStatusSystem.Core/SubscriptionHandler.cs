using System;

namespace SeiaStatusSystem.Core
{
    public class SubscriptionHandler<TStatusType> : IDisposable
    {
        readonly Subscription<TStatusType> subscription;
        readonly int subscriptionActionId;

        internal SubscriptionHandler(
            Subscription<TStatusType> subscription,
            int subscriptionActionId,
            Action<float> onStatusChanged
        )
        {
            this.subscriptionActionId = subscriptionActionId;
            this.subscription = subscription;

            subscription.AddSubscription(subscriptionActionId, onStatusChanged);
        }

        public void Dispose()
        {
            subscription.RemoveSubscription(subscriptionActionId);
        }
    }
}