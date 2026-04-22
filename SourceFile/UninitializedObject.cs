using System.Runtime.CompilerServices;

public static class UninitializedObject<T>
{
	public static readonly T Shared = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
}
