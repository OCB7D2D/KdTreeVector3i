using System;

namespace KdTree
{
	public struct ChebyshevMetrics<T, TArray, TArrayAccessor, TNumeric> : IMetrics<T, TArray>
		where T : IComparable<T>
		where TArray : struct, IFixedArray<T>
		where TArrayAccessor : struct, IFixedArrayAccessor<T, TArray>
		where TNumeric : struct, INumerics<T>
	{
		public T DistanceSquaredBetweenPoints(TArray a, TArray b)
		{
			var accessor = default(TArrayAccessor);
			var numeric = default(TNumeric);

			T distance = numeric.Zero;
			var dim = accessor.Length;

			for (var i = 0; i < dim; i++)
			{
				T distOnThisAxis = numeric.Subtract(accessor.At(ref a, i), accessor.At(ref b, i));
				distance = Max(distance, Abs(distOnThisAxis));
			}

			return numeric.Multiply(distance, distance);
		}

		private static T Abs(T x)
		{
			var numeric = default(TNumeric);
			if (x.CompareTo(numeric.Zero) < 0) return numeric.Subtract(numeric.Zero, x);
			return x;
		}

		private static T Max(T x, T y) => x.CompareTo(y) >= 0 ? x : y;
	}
}
