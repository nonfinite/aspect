using System;

namespace Aspect.Utility
{
    public static class MathEx
    {
        public static T Clamp<T>(this T value, T lower, T upper)
            where T : IComparable<T>
        {
            if (value.CompareTo(lower) <= 0)
            {
                return lower;
            }

            if (value.CompareTo(upper) >= 0)
            {
                return upper;
            }

            return value;
        }
    }
}
