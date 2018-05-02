using BenchmarkDotNet.Attributes;
using System;
using Tree = KdTree.FloatKdTree._2.Euclidean.Tree<int>;

namespace KdTreeBenchmark
{
	[MemoryDiagnoser]
	public class KdTreeBenchmark
	{
		private const int NumItems = 10000;
		private const int NumSearchIteration = 1000;
		private const float Min = -1000;
		private const float Max = 1000;

		private static float Next(Random rand, float min, float max) => (float)((max - min) * rand.NextDouble() + min);
		private static float Next(Random rand) => Next(rand, Min, Max);

		[Benchmark]
		public void RadialSearch()
		{
			var rand = new Random(1);
			var tree = new Tree();

			for (int i = 0; i < NumItems; i++)
			{
				var x = Next(rand);
				var y = Next(rand);

				tree.Add((x, y), i);
			}

			var list = Tree.CreateUnlimitedList();

			for (int i = 0; i < NumSearchIteration; i++)
			{
				var x = Next(rand);
				var y = Next(rand);
				var radius = Next(rand, 5, 100);

				list.Clear();
				tree.RadialSearch((x, y), radius, list);
			}
		}
	}
}
