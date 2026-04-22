using System;

namespace ULTRAKILL.Portal;

public struct PortalHandle(int instanceId, PortalSide side) : IEquatable<PortalHandle>
{
	public int instanceId = instanceId;

	public PortalSide side = side;

	public static readonly PortalHandle None = new PortalHandle(0, PortalSide.Enter);

	public readonly long PackedKey => (long)(((ulong)side << 32) | (uint)instanceId);

	public readonly PortalHandle Reverse()
	{
		return new PortalHandle(instanceId, side.Reverse());
	}

	public override readonly string ToString()
	{
		return $"PortalHandle(id: {instanceId}, side: {side})";
	}

	public override readonly int GetHashCode()
	{
		return instanceId ^ (int)side;
	}

	public override readonly bool Equals(object? obj)
	{
		if (obj is PortalHandle other)
		{
			return Equals(other);
		}
		return false;
	}

	public readonly bool Equals(PortalHandle other)
	{
		if (instanceId == other.instanceId)
		{
			return side == other.side;
		}
		return false;
	}

	public readonly bool IsValid()
	{
		return instanceId != 0;
	}

	public static bool operator ==(PortalHandle left, PortalHandle right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PortalHandle left, PortalHandle right)
	{
		return !left.Equals(right);
	}

	public readonly int CompareTo(PortalHandle other)
	{
		int num = instanceId.CompareTo(other.instanceId);
		if (num == 0)
		{
			num = side.CompareTo(other.side);
		}
		return num;
	}
}
