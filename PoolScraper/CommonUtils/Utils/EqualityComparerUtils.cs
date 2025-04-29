using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils
{
    public delegate bool Equals<in T>(T? x, T? y);
    public delegate int GetHashCode<in T>(T x);

    public static class EqualityComparerUtils
    {
        public static IEqualityComparer<T> AsEqualityComparer<T>(this (Func<T?, T?, bool> equals, Func<T, int> getHashCode) value)
            => new GenericEqualityComparer<T>((x, y) => value.equals(x, y), x => value.getHashCode(x));

        public static IEqualityComparer<T> AsEqualityComparer<T>(this (Equals<T> equals, GetHashCode<T> getHashCode) value)
            => new GenericEqualityComparer<T>(value.equals, value.getHashCode);

        private class GenericEqualityComparer<T> : IEqualityComparer<T>
        {
            public GenericEqualityComparer(Equals<T> Equals, GetHashCode<T> GetHashCode)
            {
                EqualsImpl = Equals;
                GetHashCodeImpl = GetHashCode;
            }

            private Equals<T> EqualsImpl { get; }
            private GetHashCode<T> GetHashCodeImpl { get; }

            public bool Equals(T? x, T? y) => EqualsImpl(x, y);
            public int GetHashCode(T obj) => GetHashCodeImpl(obj);
        }
    }
}

