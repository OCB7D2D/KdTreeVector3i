namespace KdTree
{
	using System;
	using System.Runtime.CompilerServices;

	public interface IFixedArray<T> { }

	public interface IFixedArrayAccessor<T, TArray>
		where TArray : struct, IFixedArray<T>
	{
		TArray New();
		Span<T> AsSpan(ref TArray array);
		ref T At(ref TArray array, int i);
		int Length { get; }
	}

	// consider using T4 template if you want higher dimensions.

	public struct Fixed1<T> : IFixedArrayAccessor<T, Fixed1<T>.Array>
	{
		public struct Array : IFixedArray<T>
		{
			public T Item1;
			public Array(T item1) => Item1 = item1;
			public static implicit operator Array(T value) => new Array(value);
		}

		public Array New() => default;
		public int Length => 1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Span<T> AsSpan(ref Array array) => new Span<T>(Unsafe.AsPointer(ref Unsafe.As<Array, T>(ref array)), 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T At(ref Array array, int i) => ref AsSpan(ref array)[i];
	}

	public struct Fixed2<T> : IFixedArrayAccessor<T, Fixed2<T>.Array>
	{
		public struct Array : IFixedArray<T>
		{
			public T Item1; public T Item2;
			public Array(T item1, T item2) => (Item1, Item2) = (item1, item2);
			public static implicit operator Array((T, T) value) => new Array(value.Item1, value.Item2);
		}

		public Array New() => default;
		public int Length => 2;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Span<T> AsSpan(ref Array array) => new Span<T>(Unsafe.AsPointer(ref Unsafe.As<Array, T>(ref array)), 2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T At(ref Array array, int i) => ref AsSpan(ref array)[i];
	}

	public struct Fixed3<T> : IFixedArrayAccessor<T, Fixed3<T>.Array>
	{
		public struct Array : IFixedArray<T>
		{
			public T Item1; public T Item2; public T Item3;
			public Array(T item1, T item2, T item3) => (Item1, Item2, Item3) = (item1, item2, item3);
			public static implicit operator Array((T, T, T) value) => new Array(value.Item1, value.Item2, value.Item3);
		}

		public Array New() => default;
		public int Length => 3;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Span<T> AsSpan(ref Array array) => new Span<T>(Unsafe.AsPointer(ref Unsafe.As<Array, T>(ref array)), 3);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T At(ref Array array, int i) => ref AsSpan(ref array)[i];
	}

	public static class FixedArray
	{
		public static bool Equals<T, TArray, TArrayAccessor>(TArray x, TArray y)
			where T : IEquatable<T>
			where TArray : struct, IFixedArray<T>
			where TArrayAccessor : struct, IFixedArrayAccessor<T, TArray>
		{
			var accessor = default(TArrayAccessor);
			var dim = accessor.Length;

			for (int i = 0; i < dim; i++)
			{
				if (!accessor.At(ref x, i).Equals(accessor.At(ref y, i))) return false;
			}

			return true;
		}
	}
}
