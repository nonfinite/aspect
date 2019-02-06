using System;

namespace Aspect.Utility
{
    public sealed class LazyEx<T>
    {
        public LazyEx(Func<T> valueFactory)
        {
            mValueFactory = valueFactory;
            Reset();
        }

        private readonly Func<T> mValueFactory;
        private Lazy<T> mLazy;

        public T Value => mLazy.Value;

        public void Reset() => mLazy = new Lazy<T>(mValueFactory);
    }
}
