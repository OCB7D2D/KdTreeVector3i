using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdTree
{
	public enum AddDuplicateBehavior
	{
		Skip,
		Error,
		Update
	}

	public class DuplicateNodeError : Exception
	{
		public DuplicateNodeError()
			: base("Cannot Add Node With Duplicate Coordinates")
		{
		}
	}

	[Serializable]
	public partial class KdTree<TKey, TValue, TKeyArray, TKeyArrayAccessor, TNumerics, TMetrics> : IEnumerable<(TKeyArray Point, TValue Value)>
		where TKey : IComparable<TKey>, IEquatable<TKey>
		where TKeyArray : struct, IFixedArray<TKey>
		where TKeyArrayAccessor : struct, IFixedArrayAccessor<TKey, TKeyArray>
		where TNumerics : struct, INumerics<TKey>
		where TMetrics : struct, IMetrics<TKey, TKeyArray>
	{
		private static readonly int Dimension = default(TKeyArrayAccessor).Length;
		private static bool Equals(TKeyArray a, TKeyArray b) => FixedArray.Equals<TKey, TKeyArray, TKeyArrayAccessor>(a, b);
		private static TKey DistanceSquaredBetweenPoints(TKeyArray a, TKeyArray b) => default(TMetrics).DistanceSquaredBetweenPoints(a, b);
		private static int Compare(TKey a, TKey b) => a.CompareTo(b);

		public KdTree()
		{
			Count = 0;
		}

		public KdTree(AddDuplicateBehavior addDuplicateBehavior)
		{
			AddDuplicateBehavior = addDuplicateBehavior;
		}

		private Node root = null;

		public AddDuplicateBehavior AddDuplicateBehavior { get; private set; }

		private int Increment(int value)
		{
			value++;
			if (value >= Dimension) return 0;
			return value;
		}

		public bool Add(TKeyArray point, TValue value)
		{
			var accessor = default(TKeyArrayAccessor);
			var nodeToAdd = new Node(point, value);

			if (root == null)
			{
				root = new Node(point, value);
			}
			else
			{
				int dimension = -1;
				Node parent = root;

				do
				{
					// Increment the dimension we're searching in
					dimension = Increment(dimension);

					// Does the node we're adding have the same hyperpoint as this node?
					if (Equals(point, parent.Point))
					{
						switch (AddDuplicateBehavior)
						{
							case AddDuplicateBehavior.Skip:
								return false;

							case AddDuplicateBehavior.Error:
								throw new DuplicateNodeError();

							case AddDuplicateBehavior.Update:
								parent.Value = value;
								break;

							default:
								// Should never happen
								throw new Exception("Unexpected AddDuplicateBehavior");
						}
					}

					// Which side does this node sit under in relation to it's parent at this level?
					int compare = Compare(accessor.At(ref point, dimension), accessor.At(ref parent.Point, dimension));

					if (parent[compare] == null)
					{
						parent[compare] = nodeToAdd;
						break;
					}
					else
					{
						parent = parent[compare];
					}
				}
				while (true);
			}

			Count++;
			return true;
		}

		private void ReaddChildNodes(Node removedNode)
		{
			if (removedNode.IsLeaf)
				return;

			// The folllowing code might seem a little redundant but we're using 
			// 2 queues so we can add the child nodes back in, in (more or less) 
			// the same order they were added in the first place
			var nodesToReadd = new Queue<Node>();

			var nodesToReaddQueue = new Queue<Node>();

			if (removedNode.LeftChild != null)
				nodesToReaddQueue.Enqueue(removedNode.LeftChild);

			if (removedNode.RightChild != null)
				nodesToReaddQueue.Enqueue(removedNode.RightChild);

			while (nodesToReaddQueue.Count > 0)
			{
				var nodeToReadd = nodesToReaddQueue.Dequeue();

				nodesToReadd.Enqueue(nodeToReadd);

				for (int side = -1; side <= 1; side += 2)
				{
					if (nodeToReadd[side] != null)
					{
						nodesToReaddQueue.Enqueue(nodeToReadd[side]);

						nodeToReadd[side] = null;
					}
				}
			}

			while (nodesToReadd.Count > 0)
			{
				var nodeToReadd = nodesToReadd.Dequeue();

				Count--;
				Add(nodeToReadd.Point, nodeToReadd.Value);
			}
		}

		public void RemoveAt(TKeyArray point)
		{
			// Is tree empty?
			if (root == null)
				return;

			Node node;

			if (Equals(point, root.Point))
			{
				node = root;
				root = null;
				Count--;
				ReaddChildNodes(node);
				return;
			}

			var accessor = default(TKeyArrayAccessor);
			node = root;

			int dimension = -1;
			do
			{
				dimension = Increment(dimension);

				int compare = Compare(accessor.At(ref point, dimension), accessor.At(ref node.Point, dimension));

				if (node[compare] == null)
					// Can't find node
					return;

				if (Equals(point, node[compare].Point))
				{
					var nodeToRemove = node[compare];
					node[compare] = null;
					Count--;

					ReaddChildNodes(nodeToRemove);
				}
				else
					node = node[compare];
			}
			while (node != null);
		}

		public void GetNearestNeighbours(TKeyArray point, NearestNeighbourList<(TKeyArray Key, TValue Value), TKey>.INearestNeighbourList results)
		{
			var rect = HyperRect<TKey, TKeyArray, TKeyArrayAccessor>.Infinite<TNumerics>();
			AddNearestNeighbours(root, point, rect, 0, results, default(TNumerics).MaxValue);
		}

		public (TKeyArray Key, TValue Value)[] GetNearestNeighbours(TKeyArray point, int count = int.MaxValue)
		{
			if (count > Count)
				count = Count;

			if (count < 0)
			{
				throw new ArgumentException("Number of neighbors cannot be negative");
			}

			if (count == 0)
				return Array.Empty<(TKeyArray Key, TValue Value)>();

			var nearestNeighbours = CreateNearestNeighbourList(count);

			var rect = HyperRect<TKey, TKeyArray, TKeyArrayAccessor>.Infinite<TNumerics>();

			AddNearestNeighbours(root, point, rect, 0, nearestNeighbours, default(TNumerics).MaxValue);

			count = nearestNeighbours.Count;

			return nearestNeighbours.GetSortedArray();
		}

		/*
		 * 1. Search for the target
		 * 
		 *   1.1 Start by splitting the specified hyper rect
		 *       on the specified node's point along the current
		 *       dimension so that we end up with 2 sub hyper rects
		 *       (current dimension = depth % dimensions)
		 *   
		 *	 1.2 Check what sub rectangle the the target point resides in
		 *	     under the current dimension
		 *	     
		 *   1.3 Set that rect to the nearer rect and also the corresponding 
		 *       child node to the nearest rect and node and the other rect 
		 *       and child node to the further rect and child node (for use later)
		 *       
		 *   1.4 Travel into the nearer rect and node by calling function
		 *       recursively with nearer rect and node and incrementing 
		 *       the depth
		 * 
		 * 2. Add leaf to list of nearest neighbours
		 * 
		 * 3. Walk back up tree and at each level:
		 * 
		 *    3.1 Add node to nearest neighbours if
		 *        we haven't filled our nearest neighbour
		 *        list yet or if it has a distance to target less
		 *        than any of the distances in our current nearest 
		 *        neighbours.
		 *        
		 *    3.2 If there is any point in the further rectangle that is closer to
		 *        the target than our furtherest nearest neighbour then travel into
		 *        that rect and node
		 * 
		 *  That's it, when it finally finishes traversing the branches 
		 *  it needs to we'll have our list!
		 */

		private void AddNearestNeighbours(
			Node node,
			TKeyArray target,
			HyperRect<TKey, TKeyArray, TKeyArrayAccessor> rect,
			int depth,
			NearestNeighbourList<(TKeyArray Key, TValue Value), TKey>.INearestNeighbourList nearestNeighbours,
			TKey maxSearchRadiusSquared)
		{
			if (node == null)
				return;

			// Work out the current dimension
			int dimension = depth % Dimension;

			var accessor = default(TKeyArrayAccessor);
			var nodeKey = accessor.At(ref node.Point, dimension);

			// Split our hyper-rect into 2 sub rects along the current 
			// node's point on the current dimension
			var leftRect = rect;
			accessor.At(ref leftRect.MaxPoint, dimension) = nodeKey;

			var rightRect = rect;
			accessor.At(ref rightRect.MinPoint, dimension) = nodeKey;

			// Which side does the target reside in?
			int compare = Compare(accessor.At(ref target, dimension), nodeKey);

			var nearerRect = compare <= 0 ? leftRect : rightRect;
			var furtherRect = compare <= 0 ? rightRect : leftRect;

			var nearerNode = compare <= 0 ? node.LeftChild : node.RightChild;
			var furtherNode = compare <= 0 ? node.RightChild : node.LeftChild;

			// Let's walk down into the nearer branch
			if (nearerNode != null)
			{
				AddNearestNeighbours(
					nearerNode,
					target,
					nearerRect,
					depth + 1,
					nearestNeighbours,
					maxSearchRadiusSquared);
			}

			// Walk down into the further branch but only if our capacity hasn't been reached 
			// OR if there's a region in the further rect that's closer to the target than our
			// current furtherest nearest neighbour
			TKeyArray closestPointInFurtherRect = furtherRect.GetClosestPoint(target);
			var distanceSquaredToTarget = DistanceSquaredBetweenPoints(closestPointInFurtherRect, target);

			if (Compare(distanceSquaredToTarget, maxSearchRadiusSquared) <= 0)
			{
				if (!nearestNeighbours.IsFull || Compare(distanceSquaredToTarget, nearestNeighbours.FurtherestDistance) < 0)
				{
						AddNearestNeighbours(
							furtherNode,
							target,
							furtherRect,
							depth + 1,
							nearestNeighbours,
							maxSearchRadiusSquared);
				}
			}

			// Try to add the current node to our nearest neighbours list
			distanceSquaredToTarget = DistanceSquaredBetweenPoints(node.Point, target);

			if (Compare(distanceSquaredToTarget, maxSearchRadiusSquared) <= 0)
				nearestNeighbours.Add((node.Point, node.Value), distanceSquaredToTarget);
		}

		/// <summary>
		/// Performs a radial search up to a maximum count.
		/// </summary>
		/// <param name="center">Center point</param>
		/// <param name="radius">Radius to find neighbours within</param>
		/// <param name="count">Maximum number of neighbours</param>
		public (TKeyArray Key, TValue Value)[] RadialSearch(TKeyArray center, TKey radius, int maxCapacity = int.MaxValue)
		{
			var results = CreateNearestNeighbourList(maxCapacity);
			RadialSearch(center, radius, results);

			var count = results.Count;

			return results.GetSortedArray();
		}

		public void RadialSearch(TKeyArray center, TKey radius, NearestNeighbourList<(TKeyArray Key, TValue Value), TKey>.INearestNeighbourList results)
		{
			AddNearestNeighbours(
				root,
				center,
				HyperRect<TKey, TKeyArray, TKeyArrayAccessor>.Infinite<TNumerics>(),
				0,
				results,
				default(TNumerics).Multiply(radius, radius));
		}

		public int Count { get; private set; }

		public bool TryFindValueAt(TKeyArray point, out TValue value)
		{
			var parent = root;
			int dimension = -1;
			var accessor = default(TKeyArrayAccessor);
			do
			{
				if (parent == null)
				{
					value = default;
					return false;
				}
				else if (Equals(point, parent.Point))
				{
					value = parent.Value;
					return true;
				}

				// Keep searching
				dimension = Increment(dimension);
				int compare = Compare(accessor.At(ref point, dimension), accessor.At(ref parent.Point, dimension));
				parent = parent[compare];
			}
			while (true);
		}

		public TValue FindValueAt(TKeyArray point)
		{
			if (TryFindValueAt(point, out TValue value))
				return value;
			else
				return default;
		}

		public bool TryFindValue(TValue value, out TKeyArray point)
		{
			if (root == null)
			{
				point = default;
				return false;
			}

			// First-in, First-out list of nodes to search
			var nodesToSearch = new Queue<Node>();

			nodesToSearch.Enqueue(root);

			while (nodesToSearch.Count > 0)
			{
				var nodeToSearch = nodesToSearch.Dequeue();

				if (nodeToSearch.Value.Equals(value))
				{
					point = nodeToSearch.Point;
					return true;
				}
				else
				{
					for (int side = -1; side <= 1; side += 2)
					{
						var childNode = nodeToSearch[side];

						if (childNode != null)
							nodesToSearch.Enqueue(childNode);
					}
				}
			}

			point = default;
			return false;
		}

		public TKeyArray FindValue(TValue value)
		{
			if (TryFindValue(value, out TKeyArray point))
				return point;
			else
				return default;
		}

		private void AddNodeToStringBuilder(Node node, StringBuilder sb, int depth)
		{
			sb.AppendLine(node.ToString());

			for (var side = -1; side <= 1; side += 2)
			{
				for (var index = 0; index <= depth; index++)
					sb.Append("\t");

				sb.Append(side == -1 ? "L " : "R ");

				if (node[side] == null)
					sb.AppendLine("");
				else
					AddNodeToStringBuilder(node[side], sb, depth + 1);
			}
		}

		public override string ToString()
		{
			if (root == null)
				return "";

			var sb = new StringBuilder();
			AddNodeToStringBuilder(root, sb, 0);
			return sb.ToString();
		}

		private void AddNodesToList(Node node, List<Node> nodes)
		{
			if (node == null)
				return;

			nodes.Add(node);

			for (var side = -1; side <= 1; side += 2)
			{
				if (node[side] != null)
				{
					AddNodesToList(node[side], nodes);
					node[side] = null;
				}
			}
		}

		private void SortNodesArray(Node[] nodes, int byDimension, int fromIndex, int toIndex)
		{
			var accessor = default(TKeyArrayAccessor);
			for (var index = fromIndex + 1; index <= toIndex; index++)
			{
				var newIndex = index;

				while (true)
				{
					var a = nodes[newIndex - 1];
					var b = nodes[newIndex];
					if (Compare(accessor.At(ref b.Point, byDimension), accessor.At(ref a.Point, byDimension)) < 0)
					{
						nodes[newIndex - 1] = b;
						nodes[newIndex] = a;
					}
					else
						break;
				}
			}
		}

		private void AddNodesBalanced(Node[] nodes, int byDimension, int fromIndex, int toIndex)
		{
			if (fromIndex == toIndex)
			{
				Add(nodes[fromIndex].Point, nodes[fromIndex].Value);
				nodes[fromIndex] = null;
				return;
			}

			// Sort the array from the fromIndex to the toIndex
			SortNodesArray(nodes, byDimension, fromIndex, toIndex);

			// Find the splitting point
			int midIndex = fromIndex + (int)System.Math.Round((toIndex + 1 - fromIndex) / 2f) - 1;

			// Add the splitting point
			Add(nodes[midIndex].Point, nodes[midIndex].Value);
			nodes[midIndex] = null;

			// Recurse
			int nextDimension = Increment(byDimension);

			if (fromIndex < midIndex)
				AddNodesBalanced(nodes, nextDimension, fromIndex, midIndex - 1);

			if (toIndex > midIndex)
				AddNodesBalanced(nodes, nextDimension, midIndex + 1, toIndex);
		}

		public void Balance()
		{
			var nodeList = new List<Node>();
			AddNodesToList(root, nodeList);

			Clear();

			AddNodesBalanced(nodeList.ToArray(), 0, 0, nodeList.Count - 1);
		}

		private void RemoveChildNodes(Node node)
		{
			for (var side = -1; side <= 1; side += 2)
			{
				if (node[side] != null)
				{
					RemoveChildNodes(node[side]);
					node[side] = null;
				}
			}
		}

		public void Clear()
		{
			if (root != null)
				RemoveChildNodes(root);
		}

		public IEnumerator<(TKeyArray Point, TValue Value)> GetEnumerator()
		{
			var left = new Stack<Node>();
			var right = new Stack<Node>();

			void addLeft(Node node)
			{
				if (node.LeftChild != null)
				{
					left.Push(node.LeftChild);
				}
			}

			void addRight(Node node)
			{
				if (node.RightChild != null)
				{
					right.Push(node.RightChild);
				}
			}

			if (root != null)
			{
				yield return (root.Point, root.Value);

				addLeft(root);
				addRight(root);

				while (true)
				{
					if (left.Any())
					{
						var item = left.Pop();

						addLeft(item);
						addRight(item);

						yield return (item.Point, item.Value);
					}
					else if (right.Any())
					{
						var item = right.Pop();

						addLeft(item);
						addRight(item);

						yield return (item.Point, item.Value);
					}
					else
					{
						break;
					}
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}