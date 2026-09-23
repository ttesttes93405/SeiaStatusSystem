using System;
using NUnit.Framework;
using SeiaStatusSystem.Core;

namespace SeiaStatusSystem.Core.Tests
{
    [TestFixture]
    public class StatusScopePendingEffectTests
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
        public void MultiplePendingEffects_AllApplyAndCleanOnRemoval()
        {
            using var scope = CreateScope();
            var token = scope.Apply(new TargetToken(1), new TestStatusInfo());
            int firstApplied = 0, secondApplied = 0;
            int firstCleaned = 0, secondCleaned = 0;

            scope.SubscribeEffect(token, () => { firstApplied++; return () => firstCleaned++; });
            scope.SubscribeEffect(token, () => { secondApplied++; return () => secondCleaned++; });

            scope.Dash();
            scope.RemoveByEntityToken(token);
            scope.Dash();

            Assert.Multiple(() =>
            {
                Assert.That(firstApplied, Is.EqualTo(1));
                Assert.That(secondApplied, Is.EqualTo(1));
                Assert.That(firstCleaned, Is.EqualTo(1));
                Assert.That(secondCleaned, Is.EqualTo(1));
            });
        }

        [Test]
        public void DisposingOnePendingEffect_LeavesOtherEffectSubscribed()
        {
            using var scope = CreateScope();
            var token = scope.Apply(new TargetToken(2), new TestStatusInfo());
            int firstApplied = 0, secondApplied = 0;

            var first = scope.SubscribeEffect(token, () => { firstApplied++; return () => { }; });
            scope.SubscribeEffect(token, () => { secondApplied++; return () => { }; });
            first.Dispose();

            scope.Dash();

            Assert.Multiple(() =>
            {
                Assert.That(firstApplied, Is.Zero);
                Assert.That(secondApplied, Is.EqualTo(1));
            });
        }

        [Test]
        public void RemovingPendingStatus_DoesNotApplyAnyEffect()
        {
            using var scope = CreateScope();
            var token = scope.Apply(new TargetToken(3), new TestStatusInfo());
            int applied = 0;

            scope.SubscribeEffect(token, () => { applied++; return () => { }; });
            scope.SubscribeEffect(token, () => { applied++; return () => { }; });
            scope.RemoveByEntityToken(token);
            scope.Dash();

            Assert.That(applied, Is.Zero);
        }
    }
}
