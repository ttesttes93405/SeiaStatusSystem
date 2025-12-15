using System;

namespace SeiaStatusSystem.Core
{

    public readonly struct ModifierT1<TStatusType, TStatusInfo>
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {
        public TargetToken TargetToken { get; init; }
        public TStatusType StatusType1 { get; init; }
        public Func<float, float> CalculateValue { get; init; }

        public ModifierT1(
            TargetToken targetToken,
            TStatusType statusType1,
            Func<float, float> onStatusChanged
        )
        {
            TargetToken = targetToken;
            StatusType1 = statusType1;
            CalculateValue = onStatusChanged;
        }

        public readonly float Get(StatusScope<TStatusType, TStatusInfo> statusScope, out float value)
        {
            float v1 = statusScope.GetStatusValue(TargetToken, StatusType1);
            value = v1;
            return CalculateValue(v1);
        }
    }

    public readonly struct ModifierT2<TStatusType, TStatusInfo>
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {
        public TargetToken TargetToken { get; init; }
        public TStatusType StatusType1 { get; init; }
        public TStatusType StatusType2 { get; init; }
        public Func<float, float, float> CalculateValue { get; init; }

        public ModifierT2(
            TargetToken targetToken,
            TStatusType statusType1,
            TStatusType statusType2,
            Func<float, float, float> onStatusChanged
        )
        {
            TargetToken = targetToken;
            StatusType1 = statusType1;
            StatusType2 = statusType2;
            CalculateValue = onStatusChanged;
        }

        public readonly float Get(StatusScope<TStatusType, TStatusInfo> statusScope, out (float, float) values)
        {
            float v1 = statusScope.GetStatusValue(TargetToken, StatusType1);
            float v2 = statusScope.GetStatusValue(TargetToken, StatusType2);
            values = (v1, v2);
            return CalculateValue(v1, v2);
        }
    }

    public readonly struct ModifierT3<TStatusType, TStatusInfo>
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {
        public TargetToken TargetToken { get; init; }
        public TStatusType StatusType1 { get; init; }
        public TStatusType StatusType2 { get; init; }
        public TStatusType StatusType3 { get; init; }
        public Func<float, float, float, float> CalculateValue { get; init; }

        public ModifierT3(
            TargetToken targetToken,
            TStatusType statusType1,
            TStatusType statusType2,
            TStatusType statusType3,
            Func<float, float, float, float> onStatusChanged
        )
        {
            TargetToken = targetToken;
            StatusType1 = statusType1;
            StatusType2 = statusType2;
            StatusType3 = statusType3;
            CalculateValue = onStatusChanged;
        }

        public readonly float Get(StatusScope<TStatusType, TStatusInfo> statusScope, out (float, float, float) values)
        {
            float v1 = statusScope.GetStatusValue(TargetToken, StatusType1);
            float v2 = statusScope.GetStatusValue(TargetToken, StatusType2);
            float v3 = statusScope.GetStatusValue(TargetToken, StatusType3);
            values = (v1, v2, v3);
            return CalculateValue(v1, v2, v3);
        }
    }

    public readonly struct ModifierT4<TStatusType, TStatusInfo>
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {
        public TargetToken TargetToken { get; init; }
        public TStatusType StatusType1 { get; init; }
        public TStatusType StatusType2 { get; init; }
        public TStatusType StatusType3 { get; init; }
        public TStatusType StatusType4 { get; init; }
        public Func<float, float, float, float, float> CalculateValue { get; init; }

        public ModifierT4(
            TargetToken targetToken,
            TStatusType statusType1,
            TStatusType statusType2,
            TStatusType statusType3,
            TStatusType statusType4,
            Func<float, float, float, float, float> onStatusChanged
        )
        {
            TargetToken = targetToken;
            StatusType1 = statusType1;
            StatusType2 = statusType2;
            StatusType3 = statusType3;
            StatusType4 = statusType4;
            CalculateValue = onStatusChanged;
        }

        public readonly float Get(StatusScope<TStatusType, TStatusInfo> statusScope, out (float, float, float, float) values)
        {
            float v1 = statusScope.GetStatusValue(TargetToken, StatusType1);
            float v2 = statusScope.GetStatusValue(TargetToken, StatusType2);
            float v3 = statusScope.GetStatusValue(TargetToken, StatusType3);
            float v4 = statusScope.GetStatusValue(TargetToken, StatusType4);
            values = (v1, v2, v3, v4);
            return CalculateValue(v1, v2, v3, v4);
        }
    }


}