using System;

namespace SeiaStatusSystem.Core
{
    public interface IStatusInfo<TStatusType>
    {
        public TStatusType Type { get; init; }
        public TimeSpan? Duration { get; init; }
        public float Value { get; init; }
        public Tag Tag { get; init; }
    }
}