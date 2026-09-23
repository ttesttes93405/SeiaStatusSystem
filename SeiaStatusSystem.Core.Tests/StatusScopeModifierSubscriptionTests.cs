using System;
using System.Collections.Generic;
using NUnit.Framework;
using SeiaStatusSystem.Core;

namespace SeiaStatusSystem.Core.Tests
{
    [TestFixture]
    public class StatusScopeModifierSubscriptionTests
    {
        enum TestStatusType { First, Second, Third, Fourth }

        readonly struct TestStatusInfo : IStatusInfo<TestStatusType>
        {
            public TestStatusType Type { get; init; }
            public TimeSpan? Duration { get; init; }
            public float Value { get; init; }
            public Tag Tag { get; init; }

            public TestStatusInfo(TestStatusType type, float value)
            {
                Type = type;
                Duration = null;
                Value = value;
                Tag = Tag.None;
            }
        }

        [TestCase(2, 33f)]
        [TestCase(3, 66f)]
        [TestCase(4, 110f)]
        public void MultiInputModifier_UsesAllCurrentValuesWhenSeveralStatusesChange(int inputCount, float expected)
        {
            using var scope = new StatusSystem<TestStatusType, TestStatusInfo>().CreateScope();
            var target = new TargetToken(1);
            var types = new[] { TestStatusType.First, TestStatusType.Second, TestStatusType.Third, TestStatusType.Fourth };
            var observed = new List<float>();

            for (int i = 0; i < inputCount; i++)
                scope.Apply(target, new TestStatusInfo(types[i], (i + 1) * 10));
            scope.Update(TimeSpan.Zero);

            using var subscription = inputCount switch
            {
                2 => scope.Subscribe(new ModifierT2<TestStatusType, TestStatusInfo>(target, types[0], types[1],
                    (a, b) => { observed.Add(a + b); return a + b; })),
                3 => scope.Subscribe(new ModifierT3<TestStatusType, TestStatusInfo>(target, types[0], types[1], types[2],
                    (a, b, c) => { observed.Add(a + b + c); return a + b + c; })),
                4 => scope.Subscribe(new ModifierT4<TestStatusType, TestStatusInfo>(target, types[0], types[1], types[2], types[3],
                    (a, b, c, d) => { observed.Add(a + b + c + d); return a + b + c + d; })),
                _ => throw new ArgumentOutOfRangeException(nameof(inputCount))
            };

            Assert.That(observed, Is.EqualTo(new[] { (float)(inputCount * (inputCount + 1) * 5) }));
            observed.Clear();

            for (int i = 0; i < inputCount; i++)
                scope.Apply(target, new TestStatusInfo(types[i], i + 1));
            scope.Update(TimeSpan.FromTicks(1));

            Assert.That(observed, Has.Count.EqualTo(inputCount));
            Assert.That(observed, Has.All.EqualTo(expected));
        }

        [Test]
        public void MultiInputModifier_DoesNotCalculateAtSubscribeWhenDisabled()
        {
            using var scope = new StatusSystem<TestStatusType, TestStatusInfo>().CreateScope();
            var target = new TargetToken(1);
            int calculationCount = 0;
            using var subscription = scope.Subscribe(
                new ModifierT2<TestStatusType, TestStatusInfo>(target, TestStatusType.First, TestStatusType.Second,
                    (a, b) => { calculationCount++; return a + b; }),
                executeAfterSubscribe: false);

            Assert.That(calculationCount, Is.Zero);
        }

        [Test]
        public void SingleInputModifier_CalculatesOnceOnlyWhenRequestedAtSubscribe()
        {
            using var scope = new StatusSystem<TestStatusType, TestStatusInfo>().CreateScope();
            var target = new TargetToken(1);
            int calculationCount = 0;
            var modifier = new ModifierT1<TestStatusType, TestStatusInfo>(target, TestStatusType.First,
                value => { calculationCount++; return value; });

            using var first = scope.Subscribe(modifier, executeAfterSubscribe: false);
            Assert.That(calculationCount, Is.Zero);

            using var second = scope.Subscribe(modifier);
            Assert.That(calculationCount, Is.EqualTo(1));
        }
    }
}
