namespace KdTree
{
	public partial class Point<T, TArithmetic>
	{
		public partial class Dimention<TArray, TArrayAccessor>
		{
			public struct HyperRect
			{
				public TArray MinPoint;
				public TArray MaxPoint;

				public HyperRect(TArray minPoint, TArray maxPoint) => (MinPoint, MaxPoint) = (minPoint, maxPoint);

				private static readonly int Dimension = default(TArrayAccessor).Length;

				static HyperRect()
				{
					var accessor = default(TArrayAccessor);
					var arithmetic = default(TArithmetic);
					var dim = Dimension;

					var rect = new HyperRect();

					for (int i = 0; i < dim; i++)
					{
						accessor.At(ref rect.MinPoint, i) = arithmetic.NegativeInfinity;
						accessor.At(ref rect.MaxPoint, i) = arithmetic.PositiveInfinity;
					}

					Infinite = rect;
				}

				public static readonly HyperRect Infinite;

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
	}
}
