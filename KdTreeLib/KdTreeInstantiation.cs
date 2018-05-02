using System;

namespace KdTree
{
	public partial class KdTree<T, TNumerics>
		where T : IComparable<T>, IEquatable<T>
		where TNumerics : struct, INumerics<T>
	{
		public partial class Dimention<TArray, TArrayAccessor>
			where TArray : struct, IFixedArray<T>
			where TArrayAccessor : struct, IFixedArrayAccessor<T, TArray>
		{
			public partial class Metric<TMetric>
				where TMetric : struct, IMetric<T, TArray>
			{
			}

			public class Euclidean : Metric<EuclideanMetric<T, TArray, TArrayAccessor, TNumerics>> { }
			public class Manhattan : Metric<ManhattanMetric<T, TArray, TArrayAccessor, TNumerics>> { }
			public class Chebyshev : Metric<ChebyshevMetric<T, TArray, TArrayAccessor, TNumerics>> { }
		}

		public class _1 : Dimention<Fixed1<T>.Array, Fixed1<T>> { }
		public class _2 : Dimention<Fixed2<T>.Array, Fixed2<T>> { }
		public class _3 : Dimention<Fixed3<T>.Array, Fixed3<T>> { }
	}

	public class IntKdTree : KdTree<int, IntNumerics> { }
	public class ShortKdTree : KdTree<short, ShortNumerics> { }
	public class LongKdTree : KdTree<long, LongNumerics> { }
	public class FloatKdTree : KdTree<float, FloatNumerics> { }
	public class DoubleKdTree : KdTree<double, DoubleNumerics> { }
}