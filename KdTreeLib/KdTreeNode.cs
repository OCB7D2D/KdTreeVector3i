using System;
using System.Text;

namespace KdTree
{
	public partial class KdTree<TKey, TValue, TKeyArray, TKeyArrayAccessor, TNumerics, TMetrics>
	{
		[Serializable]
		public class Node
		{
			public Node(TKeyArray point, TValue value)
			{
				Point = point;
				Value = value;
			}

			public TKeyArray Point;
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

				var accessor = default(TKeyArrayAccessor);
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