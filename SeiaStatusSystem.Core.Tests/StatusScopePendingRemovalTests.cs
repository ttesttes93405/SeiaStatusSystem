using System;
using NUnit.Framework;
using SeiaStatusSystem.Core;

namespace SeiaStatusSystem.Core.Tests
{
    [TestFixture]
    public class StatusScopePendingRemovalTests
    {
        enum TestStatusType { Attack }

        readonly struct TestStatusInfo : IStatusInfo<TestStatusType>
        {
            public TestStatusType Type { get; init; }
            public TimeSpan? Duration { get; init; }
            public float Value { get; init; }
            public Tag Tag { get; init; }

            public TestStatusInfo(float value, Tag tag)
            {
                Type = TestStatusType.Attack;
                Duration = null;
                Value = value;
                Tag = tag;
            }
        }

        static StatusScope<TestStatusType, TestStatusInfo> CreateScope() =>
            new StatusSystem<TestStatusType, TestStatusInfo>().CreateScope();

        [Test]
        public void RemoveByTag_CancelsPendingStatusWithoutPublishingEffects()
        {
            using var scope = CreateScope();
            var target = new TargetToken(1);
            var tag = new Tag(1);
            var token = scope.Apply(target, new TestStatusInfo(10, tag));
            int applied = 0;
            int cleaned = 0;
            scope.SubscribeEffect(token, () => { applied++; return () => cleaned++; });

            scope.RemoveByTag(target, tag);
            scope.Update(TimeSpan.Zero);

            Assert.Multiple(() =>
            {
                Assert.That(scope.IsEntityPending(token), Is.False);
                Assert.That(scope.IsEntityAlive(token), Is.False);
                Assert.That(scope.GetStatusValue(target, TestStatusType.Attack), Is.EqualTo(0));
                Assert.That(applied, Is.EqualTo(0));
                Assert.That(cleaned, Is.EqualTo(0));
            });
        }

        [Test]
        public void RemoveByEntityToken_CancelsPendingStatus()
        {
            using var scope = CreateScope();
            var target = new TargetToken(2);
            var token = scope.Apply(target, new TestStatusInfo(10, new Tag(2)));

            scope.RemoveByEntityToken(token);
            scope.Update(TimeSpan.Zero);

            Assert.That(scope.IsEntityAlive(token), Is.False);
            Assert.That(scope.GetStatusValue(target, TestStatusType.Attack), Is.EqualTo(0));
        }

        [Test]
        public void CleanTarget_CancelsAllPendingStatusesForTargetOnly()
        {
            using var scope = CreateScope();
            var cleanedTarget = new TargetToken(3);
            var retainedTarget = new TargetToken(4);
            var first = scope.Apply(cleanedTarget, new TestStatusInfo(10, new Tag(3)));
            var second = scope.Apply(cleanedTarget, new TestStatusInfo(20, new Tag(4)));
            var retained = scope.Apply(retainedTarget, new TestStatusInfo(30, new Tag(3)));

            scope.CleanTarget(cleanedTarget);
            scope.Update(TimeSpan.Zero);

            Assert.Multiple(() =>
            {
                Assert.That(scope.IsEntityAlive(first), Is.False);
                Assert.That(scope.IsEntityAlive(second), Is.False);
                Assert.That(scope.IsEntityAlive(retained), Is.True);
                Assert.That(scope.GetStatusValue(cleanedTarget, TestStatusType.Attack), Is.EqualTo(0));
                Assert.That(scope.GetStatusValue(retainedTarget, TestStatusType.Attack), Is.EqualTo(30));
            });
        }

        [Test]
        public void RemoveByTag_RemovesLiveAndPendingStatusesWithTheSameTag()
        {
            using var scope = CreateScope();
            var target = new TargetToken(5);
            var tag = new Tag(5);
            var live = scope.Apply(target, new TestStatusInfo(10, tag));
            int liveApplied = 0;
            int liveCleaned = 0;
            scope.SubscribeEffect(live, () => { liveApplied++; return () => liveCleaned++; });
            scope.Update(TimeSpan.Zero);

            var pending = scope.Apply(target, new TestStatusInfo(20, tag));
            int pendingApplied = 0;
            scope.SubscribeEffect(pending, () => { pendingApplied++; return () => { }; });
            scope.RemoveByTag(target, tag);
            scope.Update(TimeSpan.FromTicks(1));

            Assert.Multiple(() =>
            {
                Assert.That(scope.IsEntityAlive(live), Is.False);
                Assert.That(scope.IsEntityAlive(pending), Is.False);
                Assert.That(scope.GetStatusValue(target, TestStatusType.Attack), Is.EqualTo(0));
                Assert.That(liveApplied, Is.EqualTo(1));
                Assert.That(liveCleaned, Is.EqualTo(1));
                Assert.That(pendingApplied, Is.EqualTo(0));
            });
        }

        [Test]
        public void RemoveByTag_DoesNotRemoveStatusesWithAnotherTagOrTarget()
        {
            using var scope = CreateScope();
            var target = new TargetToken(6);
            var otherTarget = new TargetToken(7);
            var removed = scope.Apply(target, new TestStatusInfo(10, new Tag(6)));
            var differentTag = scope.Apply(target, new TestStatusInfo(20, new Tag(7)));
            var differentTarget = scope.Apply(otherTarget, new TestStatusInfo(30, new Tag(6)));

            scope.RemoveByTag(target, new Tag(6));
            scope.Update(TimeSpan.Zero);

            Assert.Multiple(() =>
            {
                Assert.That(scope.IsEntityAlive(removed), Is.False);
                Assert.That(scope.IsEntityAlive(differentTag), Is.True);
                Assert.That(scope.IsEntityAlive(differentTarget), Is.True);
            });
        }

        [Test]
        public void RemoveByTag_WithNoneTag_ThrowsBeforeLookingUpTheTarget()
        {
            using var scope = CreateScope();

            var exception = Assert.Throws<ArgumentException>(() => scope.RemoveByTag(new TargetToken(999), Tag.None));

            Assert.That(exception!.ParamName, Is.EqualTo("tag"));
        }

        [Test]
        public void RepeatedRemoval_IsIdempotentAndCleansLiveEffectOnce()
        {
            using var scope = CreateScope();
            var target = new TargetToken(8);
            var token = scope.Apply(target, new TestStatusInfo(10, new Tag(8)));
            int cleaned = 0;
            scope.SubscribeEffect(token, () => () => cleaned++);
            scope.Update(TimeSpan.Zero);

            scope.RemoveByEntityToken(token);
            scope.RemoveByEntityToken(token);
            scope.Update(TimeSpan.FromTicks(1));

            Assert.That(cleaned, Is.EqualTo(1));
        }
    }
}
