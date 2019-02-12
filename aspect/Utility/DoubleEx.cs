namespace Aspect.Utility
{
    public static class DoubleEx
    {
        public static double IfNaN(this double value, double other) => double.IsNaN(value) ? other : value;
    }
}
