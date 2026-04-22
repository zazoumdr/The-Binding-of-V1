using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class CollectionExtensions
{
	public static NativeArray<T> AsNativeArrayUnsafe<T>(this T[] array, out GCHandle handle) where T : unmanaged
	{
		return array.AsNativeArrayUnsafe(0, array.Length, out handle);
	}

	public static NativeArray<T> AsNativeArrayUnsafe<T>(this List<T> list, out GCHandle handle) where T : unmanaged
	{
		return ListExtensions.GetArrayUnsafe<T>(list).AsNativeArrayUnsafe(0, list.Count, out handle);
	}

	public unsafe static NativeArray<T> AsNativeArrayUnsafe<T>(this T[] array, int offset, int length, out GCHandle handle) where T : unmanaged
	{
		handle = GCHandle.Alloc(array, GCHandleType.Pinned);
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((byte*)(void*)handle.AddrOfPinnedObject() + (nint)offset * (nint)sizeof(T), length, Allocator.None);
	}
}
