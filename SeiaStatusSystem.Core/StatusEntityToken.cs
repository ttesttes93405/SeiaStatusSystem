

namespace SeiaStatusSystem.Core
{
    public readonly struct StatusEntityToken
    {
        public int Value { get; init; }

        internal StatusEntityToken(int value)
        {
            Value = value;
        }

        public static StatusEntityToken None => new(0);


        public override string ToString() => Value switch
        {
            0 => $"{nameof(StatusEntityToken)}(None)",
            _ => $"{nameof(StatusEntityToken)}({Value:X8})",
        };

        public override bool Equals(object obj) => obj switch
        {
            StatusEntityToken token => token.Value == Value,
            _ => false,
        };

        public override int GetHashCode() => Value.GetHashCode();
        // public static implicit operator int(StatusEntityToken token) => token.Value;
        // public static implicit operator StatusEntityToken(int value) => new(value);
        public static bool operator ==(StatusEntityToken a, StatusEntityToken b) => a.Value == b.Value;
        public static bool operator !=(StatusEntityToken a, StatusEntityToken b) => a.Value != b.Value;

    }
}