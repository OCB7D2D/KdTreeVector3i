using System;
using System.Runtime.CompilerServices;

namespace KdTree
{
	public struct GeoLocation : IFixedArray<float>
	{
		public float Latitude;
		public float Longitude;

		public GeoLocation(float latitude, float longitude) => (Latitude, Longitude) = (latitude, longitude);

		public float this[int index]
		{
			get => Unsafe.Add(ref Unsafe.As<GeoLocation, float>(ref this), index);
			set => Unsafe.Add(ref Unsafe.As<GeoLocation, float>(ref this), index) = value;
		}

		public int Length => 2;
	}

	public struct GeoLocationAccessor : IFixedArrayAccessor<float, GeoLocation>
	{
		public GeoLocation New() => default;
		public int Length => 2;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Span<float> AsSpan(ref GeoLocation array) => new Span<float>(Unsafe.AsPointer(ref Unsafe.As<GeoLocation, float>(ref array)), 2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref float At(ref GeoLocation array, int i) => ref AsSpan(ref array)[i];
	}

	public struct GeoMetic : IMetric<float, GeoLocation>
	{
		public float DistanceSquared(GeoLocation a, GeoLocation b)
		{
			double dst = GeoUtils.Distance(a.Latitude, a.Longitude, b.Latitude, b.Longitude, 'K');
			return (float)(dst * dst);
		}

		public bool Equals(GeoLocation x, GeoLocation y) => x.Latitude == y.Latitude && x.Longitude == y.Longitude;
		public int GetHashCode(GeoLocation obj) => obj.GetHashCode();
	}
}
