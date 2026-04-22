using System;
using System.Runtime.CompilerServices;

public static class EnumExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static bool HasAnyFlag<T>(this T self, T flags) where T : unmanaged, Enum
	{
		if (sizeof(T) == 8)
		{
			return HasAnyFlag(*(ulong*)(&self), *(ulong*)(&flags));
		}
		if (sizeof(T) == 4)
		{
			return HasAnyFlag(*(uint*)(&self), *(uint*)(&flags));
		}
		if (sizeof(T) == 2)
		{
			return HasAnyFlag(*(ushort*)(&self), *(ushort*)(&flags));
		}
		return HasAnyFlag(*(byte*)(&self), *(byte*)(&flags));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static bool HasAllFlags<T>(this T self, T flags) where T : unmanaged, Enum
	{
		if (sizeof(T) == 8)
		{
			return HasAllFlags(*(ulong*)(&self), *(ulong*)(&flags));
		}
		if (sizeof(T) == 4)
		{
			return HasAllFlags(*(uint*)(&self), *(uint*)(&flags));
		}
		if (sizeof(T) == 2)
		{
			return HasAllFlags(*(ushort*)(&self), *(ushort*)(&flags));
		}
		return HasAllFlags(*(byte*)(&self), *(byte*)(&flags));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAnyFlag(byte a, byte b)
	{
		return (a & b) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAnyFlag(ushort a, ushort b)
	{
		return (a & b) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAnyFlag(uint a, uint b)
	{
		return (a & b) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAnyFlag(ulong a, ulong b)
	{
		return (a & b) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAllFlags(byte a, byte b)
	{
		return (a & b) == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAllFlags(ushort a, ushort b)
	{
		return (a & b) == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAllFlags(uint a, uint b)
	{
		return (a & b) == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasAllFlags(ulong a, ulong b)
	{
		return (a & b) == b;
	}
}
