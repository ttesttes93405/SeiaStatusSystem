

namespace SeiaStatusSystem.Core
{
    public readonly struct TargetToken
    {
        public int Value { get; init; }

        public TargetToken(int value)
        {
            Value = value;
        }

        public static TargetToken None => new(0);

        public static TargetToken GenerateNew()
        {
            int newValue = Utilities.GenerateUniqueId();
            return new TargetToken(newValue);
        }

        public override string ToString() => Value switch
        {
            0 => $"{nameof(TargetToken)}(None)",
            _ => $"{nameof(TargetToken)}({Value:X8})",
        };

        public override bool Equals(object obj) => obj switch
        {
            TargetToken token => token.Value == Value,
            _ => false,
        };

        public override int GetHashCode() => Value.GetHashCode();
        public static implicit operator int(TargetToken token) => token.Value;
        public static implicit operator TargetToken(int value) => new(value);
        public static bool operator ==(TargetToken a, TargetToken b) => a.Value == b.Value;
        public static bool operator !=(TargetToken a, TargetToken b) => a.Value != b.Value;

    }
}