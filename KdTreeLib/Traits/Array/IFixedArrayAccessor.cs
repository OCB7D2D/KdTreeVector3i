using System;

namespace KdTree
{
	public interface IFixedArrayAccessor<T, TArray>
		where TArray : struct, IFixedArray<T>
	{
		TArray New();
		Span<T> AsSpan(ref TArray array);
		ref T At(ref TArray array, int i);
		int Length { get; }
	}
}
