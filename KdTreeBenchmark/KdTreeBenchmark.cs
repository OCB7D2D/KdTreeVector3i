using BenchmarkDotNet.Attributes;
using System;
using Tree = KdTree3.KdTree<KdTree3.MetricChebyshev>.Vector3i<int>;

namespace KdTreeBenchmark
{
	[MemoryDiagnoser]
	public class KdTreeBenchmark
	{
		private const int NumItems = 10000;
		private const int NumSearchIteration = 1000;
		private const int Min = -1000;
		private const int Max = 1000;

		[Benchmark]
		public void RadialSearch()
		{
			var rand = new Random(42);
			var tree = new Tree();

			for (int i = 0; i < NumItems; i++)
			{
				var position = new global::Vector3i(
					rand.Next(Min, Max),
					rand.Next(Min, Max),
					rand.Next(Min, Max));
				tree.Add(position, i);
			}

			var list = Tree.CreateUnlimitedList();

			int count = 0;

			for (int i = 0; i < NumSearchIteration; i++)
			{
				var position = new global::Vector3i(
					rand.Next(Min, Max),
					rand.Next(Min, Max),
					rand.Next(Min, Max));
				var radius = rand.Next(5, 100);

				list.Clear();
				tree.RadialSearch(position, radius, list);
				count += list.Count;
			}
			if (count != 2631) Console.WriteLine(count);
		}
	}
}
