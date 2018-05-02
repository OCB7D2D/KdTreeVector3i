using System.Collections.Generic;

namespace KdTree
{
	public interface IMetric<T, TArray>
		where TArray : IFixedArray<T>
	{
		T DistanceSquared(TArray a, TArray b);
	}
}
