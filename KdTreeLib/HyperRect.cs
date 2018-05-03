using System;

namespace KdTree
{
	public struct HyperRect<T, TArray, TArrayAccessor>
		where T : IComparable<T>
		where TArray : struct, IFixedArray<T>
		where TArrayAccessor : struct, IFixedArrayAccessor<T, TArray>
	{
		public TArray MinPoint;

		public TArray MaxPoint;

		public HyperRect(TArray minPoint, TArray maxPoint) => (MinPoint, MaxPoint) = (minPoint, maxPoint);

		private static readonly int Dimension = default(TArrayAccessor).Length;

		public static HyperRect<T, TArray, TArrayAccessor> Infinite<TArithmetic>()
			where TArithmetic : struct, IArithmetic<T>
		{
			var accessor = default(TArrayAccessor);
			var arithmetic = default(TArithmetic);
			var dim = Dimension;

			var rect = new HyperRect<T, TArray, TArrayAccessor>();

			for (int i = 0; i < dim; i++)
			{
				accessor.At(ref rect.MinPoint, i) = arithmetic.NegativeInfinity;
				accessor.At(ref rect.MaxPoint, i) = arithmetic.PositiveInfinity;
			}

			return rect;
		}

		public TArray GetClosestPoint(TArray toPoint)
		{
			var accessor = default(TArrayAccessor);
			var dim = Dimension;
			TArray closest = default;

			for (var dimension = 0; dimension < dim; dimension++)
			{
				var min = accessor.At(ref MinPoint, dimension);
				var max = accessor.At(ref MaxPoint, dimension);
				var to = accessor.At(ref toPoint, dimension);
				ref var c = ref accessor.At(ref closest, dimension);

				if (min.CompareTo(to) > 0)
				{
					c = min;
				}
				else if (max.CompareTo(to) < 0)
				{
					c = max;
				}
				else
					// Point is within rectangle, at least on this dimension
					c = to;
			}

			return closest;
		}
	}
}
