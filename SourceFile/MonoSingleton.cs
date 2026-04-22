using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public abstract class MonoSingleton : MonoBehaviour
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	private protected static readonly Dictionary<Type, MonoSingleton> s_Instances = new Dictionary<Type, MonoSingleton>();

	private protected MonoSingleton()
	{
	}

	public static MonoSingleton GetInstance(Type type)
	{
		s_Instances.TryGetValue(type, out MonoSingleton value);
		return value;
	}

	public abstract void MakeCurrent();
}
public abstract class MonoSingleton<T> : MonoSingleton where T : MonoSingleton<T>
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	private const int MaxFindCount = 1;

	[EditorBrowsable(EditorBrowsableState.Never)]
	private static int s_FindCount;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The 'instance' field should not be accessed directly. Please use the 'Instance' property instead.")]
	private static T? instance;

	[EditorBrowsable(EditorBrowsableState.Never)]
	private static readonly SingletonFlags flags = typeof(T).GetCustomAttribute<ConfigureSingletonAttribute>()?.Flags ?? SingletonFlags.None;

	public static T? Instance
	{
		get
		{
			return GetOrCreateInstance();
		}
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This setter is obsolete, please use MakeCurrent instead.")]
		protected set
		{
			value?.MakeCurrent();
		}
	}

	public static event Action<T>? InstanceChanged;

	[EditorBrowsable(EditorBrowsableState.Never)]
	private static T? GetOrCreateInstance()
	{
		if ((bool)instance)
		{
			return instance;
		}
		if ((object)instance != null)
		{
			s_FindCount = 0;
			instance = null;
		}
		if (s_FindCount < 1)
		{
			s_FindCount++;
			UpdateInstanceInternal(UnityEngine.Object.FindAnyObjectByType<T>());
		}
		if (flags.HasFlag(SingletonFlags.NoAutoInstance) || instance != null)
		{
			return instance;
		}
		T val = UninitializedObject<T>.Shared.CreateAutoInstance();
		if (val == null)
		{
			Debug.LogError("Auto instance creation failed for " + typeof(T).FullName + ", falling back to CreateDefaultInstance.");
			val = CreateDefaultInstance();
		}
		if (flags.HasFlag(SingletonFlags.HideAutoInstance))
		{
			val.gameObject.hideFlags |= HideFlags.HideInHierarchy;
		}
		if (flags.HasFlag(SingletonFlags.PersistAutoInstance))
		{
			UnityEngine.Object.DontDestroyOnLoad(val.gameObject);
			val.gameObject.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		}
		UpdateInstanceInternal(val);
		return instance;
	}

	[MemberNotNullWhen(true, "Instance")]
	public static bool TryGetInstance([NotNullWhen(true)] out T? instance)
	{
		instance = GetOrCreateInstance();
		return instance != null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private void __internalAwake()
	{
		if (instance != null && flags.HasFlag(SingletonFlags.DestroyDuplicates) && instance != this)
		{
			UnityEngine.Object.Destroy(this);
		}
		else if (!(instance != null) || !instance.isActiveAndEnabled || base.isActiveAndEnabled)
		{
			MakeCurrent();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private void OnEnableINTERNAL()
	{
		MakeCurrent();
	}

	public sealed override void MakeCurrent()
	{
		if (this == null)
		{
			Debug.LogWarning("MakeCurrent called on destroyed " + typeof(T).FullName + ".", this);
		}
		UpdateInstanceInternal((T)this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private static void UpdateInstanceInternal(T newInstance)
	{
		if ((object)instance == newInstance || newInstance == null || !newInstance.gameObject.scene.IsValid())
		{
			return;
		}
		instance = newInstance;
		MonoSingleton.s_Instances[typeof(T)] = newInstance;
		s_FindCount = 0;
		try
		{
			MonoSingleton<T>.InstanceChanged?.Invoke(newInstance);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("Creates an auto instance of this singleton. This method should be implemented as if it were static, never accessing instance data or methods.")]
	protected virtual T CreateAutoInstance()
	{
		return CreateDefaultInstance();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected static T CreateDefaultInstance()
	{
		return new GameObject(typeof(T).FullName).AddComponent<T>();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected static T CreateInstanceFromResources(string name)
	{
		return UnityEngine.Object.Instantiate(Resources.Load<T>(name));
	}
}
