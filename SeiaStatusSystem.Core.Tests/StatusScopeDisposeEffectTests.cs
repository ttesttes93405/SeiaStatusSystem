using System;
using NUnit.Framework;
using SeiaStatusSystem.Core;

namespace SeiaStatusSystem.Core.Tests
{
    [TestFixture]
    public class StatusScopeDisposeEffectTests
    {
        enum TestStatusType { Attack }

        readonly struct TestStatusInfo : IStatusInfo<TestStatusType>
        {
            public TestStatusType Type { get; init; }
            public TimeSpan? Duration { get; init; }
            public float Value { get; init; }
            public Tag Tag { get; init; }
        }

        static StatusScope<TestStatusType, TestStatusInfo> CreateScope() =>
            new StatusSystem<TestStatusType, TestStatusInfo>().CreateScope();

        [Test]
        public void Dispose_CleansStartedEffectsExactlyOnce_WithoutAnotherUpdate()
        {
            var scope = CreateScope();
            var token = scope.Apply(new TargetToken(1), new TestStatusInfo());
            int cleaned = 0;
            scope.SubscribeEffect(token, () => () => cleaned++);
            scope.Dash();
            scope.RemoveByEntityToken(token);

            scope.Dispose();
            scope.Dispose();

            Assert.That(cleaned, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_DoesNotStartPendingEffects()
        {
            var scope = CreateScope();
            var token = scope.Apply(new TargetToken(2), new TestStatusInfo());
            int applied = 0;
            int cleaned = 0;
            scope.SubscribeEffect(token, () =>
            {
                applied++;
                return () => cleaned++;
            });

            scope.Dispose();

            Assert.Multiple(() =>
            {
                Assert.That(applied, Is.Zero);
                Assert.That(cleaned, Is.Zero);
            });
        }

        [Test]
        public void Dispose_DoesNotRepeatCleanerAlreadyRunByRemoval()
        {
            var scope = CreateScope();
            var token = scope.Apply(new TargetToken(4), new TestStatusInfo());
            int cleaned = 0;
            scope.SubscribeEffect(token, () => () => cleaned++);
            scope.Dash();
            scope.RemoveByEntityToken(token);
            scope.Dash();

            scope.Dispose();

            Assert.That(cleaned, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ContinuesCleaningOtherEffectsWhenOneCleanerThrows()
        {
            var scope = CreateScope();
            var token = scope.Apply(new TargetToken(3), new TestStatusInfo());
            scope.Dash();
            int cleaned = 0;
            scope.SubscribeEffect(token, () => () => throw new InvalidOperationException("cleanup failed"));
            scope.SubscribeEffect(token, () => () => cleaned++);

            var exception = Assert.Throws<AggregateException>(() => scope.Dispose());

            Assert.Multiple(() =>
            {
                Assert.That(cleaned, Is.EqualTo(1));
                Assert.That(exception!.InnerExceptions, Has.Count.EqualTo(1));
                Assert.That(scope.IsDisposed, Is.True);
            });
            Assert.DoesNotThrow(() => scope.Dispose());
        }
    }
}
