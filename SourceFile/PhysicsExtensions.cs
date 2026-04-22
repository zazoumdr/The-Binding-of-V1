using UnityEngine;

public static class PhysicsExtensions
{
	public static void SetCustomGravity(this Rigidbody rigidbody, Vector3 gravity)
	{
		bool exists;
		CustomGravity orAddComponent = rigidbody.GetOrAddComponent<CustomGravity>(out exists);
		if (!exists)
		{
			orAddComponent.enabled = false;
		}
		orAddComponent.gravity = gravity;
	}

	public static bool TryGetCustomGravity(this Rigidbody rigidbody, out Vector3 gravity)
	{
		if (rigidbody.TryGetComponent<CustomGravity>(out var component))
		{
			gravity = component.gravity;
			return true;
		}
		gravity = default(Vector3);
		return false;
	}

	public static bool GetCustomGravityMode(this Rigidbody rigidbody)
	{
		if (rigidbody.TryGetComponent<CustomGravity>(out var component))
		{
			return component.enabled;
		}
		return false;
	}

	public static void SetCustomGravityMode(this Rigidbody rigidbody, bool useCustomGravity)
	{
		CustomGravity component;
		if (useCustomGravity)
		{
			rigidbody.GetOrAddComponent<CustomGravity>().enabled = true;
		}
		else if (rigidbody.TryGetComponent<CustomGravity>(out component))
		{
			component.enabled = false;
		}
	}

	public static bool GetGravityMode(this Rigidbody rigidbody)
	{
		if (rigidbody.TryGetComponent<CustomGravity>(out var component) && component.enabled)
		{
			return component.useGravity;
		}
		return rigidbody.useGravity;
	}

	public static void SetGravityMode(this Rigidbody rigidbody, bool useGravity)
	{
		if (rigidbody.TryGetComponent<CustomGravity>(out var component) && component.enabled)
		{
			component.useGravity = useGravity;
		}
		else
		{
			rigidbody.useGravity = useGravity;
		}
	}

	public static Vector3 GetGravityDirection(this Rigidbody rigidbody)
	{
		return rigidbody.GetGravityVector().normalized;
	}

	public static Vector3 GetGravityDirection(this GameObject gameObject)
	{
		return gameObject.GetGravityVector().normalized;
	}

	public static Vector3 GetGravityVector(this Rigidbody rigidbody)
	{
		if (rigidbody.TryGetComponent<CustomGravity>(out var component) && component.enabled)
		{
			return component.gravity;
		}
		return Physics.gravity;
	}

	public static Vector3 GetGravityVector(this GameObject gameObject)
	{
		if (gameObject.TryGetComponent<Rigidbody>(out var component))
		{
			return component.GetGravityVector();
		}
		return Physics.gravity;
	}
}
