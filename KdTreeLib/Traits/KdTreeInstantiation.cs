using System;

namespace KdTree
{
	public partial class KdTree<T, TArithmetic>
		where T : IComparable<T>, IEquatable<T>
		where TArithmetic : struct, IArithmetic<T>
	{
		public partial class Dimention<TArray, TArrayAccessor>
			where TArray : struct, IFixedArray<T>
			where TArrayAccessor : struct, IFixedArrayAccessor<T, TArray>
		{
			public partial class Metric<TMetric>
				where TMetric : struct, IMetric<T, TArray>
			{
			}

			public class Euclidean : Metric<EuclideanMetric<T, TArray, TArrayAccessor, TArithmetic>> { }
			public class Manhattan : Metric<ManhattanMetric<T, TArray, TArrayAccessor, TArithmetic>> { }
			public class Chebyshev : Metric<ChebyshevMetric<T, TArray, TArrayAccessor, TArithmetic>> { }
		}

		public class _1 : Dimention<Fixed1<T>.Array, Fixed1<T>> { }
		public class _2 : Dimention<Fixed2<T>.Array, Fixed2<T>> { }
		public class _3 : Dimention<Fixed3<T>.Array, Fixed3<T>> { }
		public class _4 : Dimention<Fixed4<T>.Array, Fixed4<T>> { }
	}

	public class IntKdTree : KdTree<int, IntArithmetic> { }
	public class ShortKdTree : KdTree<short, ShortArithmetic> { }
	public class LongKdTree : KdTree<long, LongArithmetic> { }
	public class FloatKdTree : KdTree<float, FloatArithmetic>
	{
		public class Geo : Dimention<GeoLocation, GeoLocationAccessor>
		{
			public class Geodesics : Metric<GeoMetic> { }
		}
	}
	public class DoubleKdTree : KdTree<double, DoubleArithmetic> { }
}