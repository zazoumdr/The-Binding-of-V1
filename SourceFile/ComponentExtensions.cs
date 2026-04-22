using System;
using UnityEngine;

public static class ComponentExtensions
{
	public static T GetOrAddComponent<T>(this Component component) where T : Component
	{
		return component.gameObject.GetOrAddComponent<T>();
	}

	public static Component GetOrAddComponent(this Component component, Type componentType)
	{
		return component.gameObject.GetOrAddComponent(componentType);
	}

	public static T GetOrAddComponent<T>(this Component component, out bool exists) where T : Component
	{
		return component.gameObject.GetOrAddComponent<T>(out exists);
	}

	public static Component GetOrAddComponent(this Component component, Type componentType, out bool exists)
	{
		return component.gameObject.GetOrAddComponent(componentType, out exists);
	}

	public static bool MoveComponentUp(this Component component)
	{
		int componentIndex = component.GetComponentIndex();
		if (componentIndex <= 1)
		{
			return false;
		}
		component.gameObject.SwapComponents(componentIndex, componentIndex - 1);
		return true;
	}

	public static bool MoveComponentDown(this Component component)
	{
		int componentIndex = component.GetComponentIndex();
		if (componentIndex == 0 || componentIndex + 1 == component.gameObject.GetComponentCount())
		{
			return false;
		}
		component.gameObject.SwapComponents(componentIndex, componentIndex + 1);
		return true;
	}
}
