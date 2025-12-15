using System;

namespace SeiaStatusSystem.Core
{
    internal readonly struct StatusValue
    {
        public readonly float TotalValue;

        public readonly TimeSpan UpdateTime;

        public StatusValue(float value, TimeSpan updateTime)
        {
            TotalValue = value;
            UpdateTime = updateTime;
        }
    }
}