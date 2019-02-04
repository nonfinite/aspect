using System;

using Aspect.Utility;

namespace Aspect.Models
{
    public struct Rating : IComparable<Rating>, IComparable, IEquatable<Rating>
    {
        public Rating(byte rating)
        {
            Value = rating.Clamp<byte>(1, 5);
        }

        public byte Value { get; }

        public int CompareTo(Rating other) => Value.CompareTo(other.Value);

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is Rating other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Rating)}");
        }

        public bool Equals(Rating other) => Value == other.Value;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Rating other && Equals(other);
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Rating left, Rating right) => left.Equals(right);

        public static bool operator !=(Rating left, Rating right) => !left.Equals(right);

        public static bool operator <(Rating left, Rating right) => left.CompareTo(right) < 0;
        public static bool operator >(Rating left, Rating right) => left.CompareTo(right) > 0;
    }
}
