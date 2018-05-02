using System;
using System.Text;

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
				public partial class Tree<TValue>
				{
					[Serializable]
					public class Node
					{
						public Node(TArray point, TValue value)
						{
							Point = point;
							Value = value;
						}

						public TArray Point;
						public TValue Value = default;

						internal Node LeftChild = null;
						internal Node RightChild = null;

						internal ref Node this[int compare]
						{
							get
							{
								if (compare <= 0)
									return ref LeftChild;
								else
									return ref RightChild;
							}
						}

						public bool IsLeaf
						{
							get
							{
								return (LeftChild == null) && (RightChild == null);
							}
						}

						public override string ToString()
						{
							var sb = new StringBuilder();

							var accessor = default(TArrayAccessor);
							var dim = accessor.Length;
							var p = Point;
							for (var i = 0; i < dim; i++)
							{
								sb.Append(accessor.At(ref p, i).ToString() + "\t");
							}

							if (Value == null)
								sb.Append("null");
							else
								sb.Append(Value.ToString());

							return sb.ToString();
						}
					}
				}
			}
		}
	}
}