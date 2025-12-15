
namespace SeiaStatusSystem.Core
{
    public readonly struct Tag
    {
        public int Value { get; init; }
        public Tag(int value)
        {
            Value = value;
        }

        public static Tag None => new(0);

        public static Tag GenerateNew()
        {
            int newValue = Utilities.GenerateUniqueId();
            return new Tag(newValue);
        }

        public override string ToString() => Value switch
        {
            0 => $"{nameof(Tag)}(None)",
            _ => $"{nameof(Tag)}({Value:X8})",
        };

        public override bool Equals(object obj) => obj switch
        {
            Tag tag => Value == tag.Value,
            _ => false,
        };

        public override int GetHashCode() => Value.GetHashCode();
        public static implicit operator int(Tag token) => token.Value;
        public static implicit operator Tag(int value) => new(value);
        public static bool operator ==(Tag a, Tag b) => a.Value == b.Value;
        public static bool operator !=(Tag a, Tag b) => a.Value != b.Value;

    }

}