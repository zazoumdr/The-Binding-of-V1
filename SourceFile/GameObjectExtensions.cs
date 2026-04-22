using System;
using System.Runtime.CompilerServices;
using Interop;
using Interop.core;
using Interop.Unity;
using PrivateAPIBridge;
using UnityEngine;

public static class GameObjectExtensions
{
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		if (gameObject.TryGetComponent<T>(out var component))
		{
			return component;
		}
		return gameObject.AddComponent<T>();
	}

	public static Component GetOrAddComponent(this GameObject gameObject, Type componentType)
	{
		if (gameObject.TryGetComponent(componentType, out var component))
		{
			return component;
		}
		return gameObject.AddComponent(componentType);
	}

	public static T GetOrAddComponent<T>(this GameObject gameObject, out bool exists) where T : Component
	{
		if (gameObject.TryGetComponent<T>(out var component))
		{
			exists = true;
			return component;
		}
		exists = false;
		return gameObject.AddComponent<T>();
	}

	public static Component GetOrAddComponent(this GameObject gameObject, Type componentType, out bool exists)
	{
		if (gameObject.TryGetComponent(componentType, out var component))
		{
			exists = true;
			return component;
		}
		exists = false;
		return gameObject.AddComponent(componentType);
	}

	public static GameObject CreateChild(this GameObject parent)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.SetParent(parent.transform, worldPositionStays: false);
		return gameObject;
	}

	public static GameObject CreateChild(this GameObject parent, string name)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.SetParent(parent.transform, worldPositionStays: false);
		return gameObject;
	}

	public static T CreateChild<T>(this GameObject parent) where T : Component
	{
		return parent.CreateChild().GetOrAddComponent<T>();
	}

	public static T CreateChild<T>(this GameObject parent, string name) where T : Component
	{
		return parent.CreateChild(name).GetOrAddComponent<T>();
	}

	public static int GetFirstAudioFilterIndex(this GameObject @this)
	{
		int i = 1;
		for (int componentCount = @this.GetComponentCount(); i < componentCount; i++)
		{
			Component componentAtIndex = @this.GetComponentAtIndex(i);
			if (componentAtIndex is AudioChorusFilter || componentAtIndex is AudioDistortionFilter || componentAtIndex is AudioEchoFilter || componentAtIndex is AudioHighPassFilter || componentAtIndex is AudioLowPassFilter || componentAtIndex is AudioReverbFilter)
			{
				return i;
			}
		}
		return -1;
	}

	public static int GetLastAudioFilterIndex(this GameObject @this)
	{
		for (int num = @this.GetComponentCount() - 1; num >= 1; num--)
		{
			Component componentAtIndex = @this.GetComponentAtIndex(num);
			if (componentAtIndex is AudioChorusFilter || componentAtIndex is AudioDistortionFilter || componentAtIndex is AudioEchoFilter || componentAtIndex is AudioHighPassFilter || componentAtIndex is AudioLowPassFilter || componentAtIndex is AudioReverbFilter)
			{
				return num;
			}
		}
		return -1;
	}

	public unsafe static void SwapComponents(this GameObject @this, int index1, int index2)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		vector<ComponentPair>* ptr = &((GameObject)(void*)ObjectExtensions.GetCachedPtr((UnityEngine.Object)@this)).m_Component;
		if (index1 < 1 || index1 >= (int)(nuint)ptr->m_size)
		{
			throw new ArgumentOutOfRangeException("index1");
		}
		if (index2 < 1 || index2 >= (int)(nuint)ptr->m_size)
		{
			throw new ArgumentOutOfRangeException("index2");
		}
		ref ComponentPair reference = ref *(ComponentPair*)((byte*)ptr->m_ptr + (nint)index2 * (nint)Unsafe.SizeOf<ComponentPair>());
		byte* num = (byte*)ptr->m_ptr + (nint)index1 * (nint)Unsafe.SizeOf<ComponentPair>();
		ComponentPair val = ((ComponentPair*)ptr->m_ptr)[index1];
		ComponentPair val2 = ((ComponentPair*)ptr->m_ptr)[index2];
		reference = val;
		Unsafe.Write(num, val2);
		UnityEngine.Object scriptingObject = GetScriptingObject<Component>(((ComponentPair)((byte*)ptr->m_ptr + (nint)index1 * (nint)Unsafe.SizeOf<ComponentPair>())).component.m_Ptr);
		UnityEngine.Object scriptingObject2 = GetScriptingObject<Component>(((ComponentPair)((byte*)ptr->m_ptr + (nint)index2 * (nint)Unsafe.SizeOf<ComponentPair>())).component.m_Ptr);
		if ((bool)scriptingObject && scriptingObject is Behaviour { enabled: not false } behaviour)
		{
			behaviour.enabled = false;
			behaviour.enabled = true;
		}
		if ((bool)scriptingObject2 && scriptingObject2 is Behaviour { enabled: not false } behaviour2)
		{
			behaviour2.enabled = false;
			behaviour2.enabled = true;
		}
	}

	private unsafe static UnityEngine.Object GetScriptingObject<T>(T* pointer) where T : unmanaged, Interface
	{
		if (pointer != null)
		{
			return Resources.InstanceIDToObject(((Object)UpCasts.static_cast<T, Object>(pointer)).m_InstanceID);
		}
		return null;
	}
}
