using System.Collections.Generic;

namespace KdTree
{
	public interface IMetrics<T, TArray>
		where TArray : IFixedArray<T>
	{
		T DistanceSquaredBetweenPoints(TArray a, TArray b);
	}
}
