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
			public partial class Metrics<TMetrics>
				where TMetrics : struct, IMetrics<T, TArray>
			{
			}

			public class Euclidean : Metrics<Math.EuclideanMetrics<T, TArray, TArrayAccessor, TNumerics>> { }
		}

		public class _1 : Dimention<Fixed1<T>.Array, Fixed1<T>> { }
		public class _2 : Dimention<Fixed2<T>.Array, Fixed2<T>> { }
		public class _3 : Dimention<Fixed3<T>.Array, Fixed3<T>> { }
	}

	public class FloatKdTreeFactory : KdTree<float, Math.FloatMath> { }
	public class DoubleKdTreeFactory : KdTree<double, Math.DoubleMath> { }
}