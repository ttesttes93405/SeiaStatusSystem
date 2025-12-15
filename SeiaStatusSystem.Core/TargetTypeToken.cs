using System;

namespace SeiaStatusSystem.Core
{
    public readonly struct TargetTypeToken<TStatusType>
        where TStatusType : Enum
    {
        public TargetToken TargetToken { get; init; }
        public TStatusType StatusType { get; init; }

        public TargetTypeToken(TargetToken targetToken, TStatusType statusType)
        {
            TargetToken = targetToken;
            StatusType = statusType;
        }

        public void Deconstruct(out TargetToken targetToken, out TStatusType statusType)
        {
            targetToken = TargetToken;
            statusType = StatusType;
        }

        public override string ToString() => $"{nameof(TargetTypeToken<TStatusType>)}(TargetToken: {TargetToken}, StatusType: {StatusType})";


        public override bool Equals(object obj) => obj switch
        {
            TargetTypeToken<TStatusType> token => token.TargetToken == TargetToken && token.StatusType.Equals(StatusType),
            _ => false,
        };

        public override int GetHashCode() => (TargetToken, StatusType).GetHashCode();
        public static bool operator ==(TargetTypeToken<TStatusType> a, TargetTypeToken<TStatusType> b) => a.TargetToken == b.TargetToken && a.StatusType.Equals(b.StatusType);
        public static bool operator !=(TargetTypeToken<TStatusType> a, TargetTypeToken<TStatusType> b) => a.TargetToken != b.TargetToken || !a.StatusType.Equals(b.StatusType);
    }

}