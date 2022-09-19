using System;

// Copied from 7D2D implementation
public struct Vector3i : IEquatable<Vector3i>
{

	public static readonly Vector3i zero = new Vector3i(0, 0, 0);

	public int x;
	public int y;
	public int z;

	public Vector3i(int _x, int _y, int _z)
	{
		this.x = _x;
		this.y = _y;
		this.z = _z;
	}

	public bool Equals(int _x, int _y, int _z) => this.x == _x && this.y == _y && this.z == _z;

	public override bool Equals(object obj) => this.Equals((Vector3i)obj);

	public bool Equals(Vector3i other) => other.x == this.x && other.y == this.y && other.z == this.z;

	public static bool operator ==(Vector3i one, Vector3i other) => one.x == other.x && one.y == other.y && one.z == other.z;

	public static bool operator !=(Vector3i one, Vector3i other) => !(one == other);

	public override int GetHashCode() => this.x * 8976890 + this.y * 981131 + this.z;

	public override string ToString() => string.Format("{0}, {1}, {2}", (object)this.x, (object)this.y, (object)this.z);

}
