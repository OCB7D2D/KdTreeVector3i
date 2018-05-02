﻿namespace KdTree
{
	public struct EuclideanMetric<T, TArray, TArrayAccessor, TNumeric> : IMetric<T, TArray>
		where TArray : struct, IFixedArray<T>
		where TArrayAccessor : struct, IFixedArrayAccessor<T, TArray>
		where TNumeric : struct, INumerics<T>
	{
		public T DistanceSquared(TArray a, TArray b)
		{
			var accessor = default(TArrayAccessor);
			var numeric = default(TNumeric);

			T distance = numeric.Zero;
			var dim = accessor.Length;

			for (var i = 0; i < dim; i++)
			{
				T distOnThisAxis = numeric.Subtract(accessor.At(ref a, i), accessor.At(ref b, i));
				T distOnThisAxisSquared = numeric.Multiply(distOnThisAxis, distOnThisAxis);
				distance = numeric.Add(distance, distOnThisAxisSquared);
			}

			return distance;
		}
	}
}