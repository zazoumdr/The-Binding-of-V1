using UnityEngine;

public class CheckForScroller : MonoBehaviour
{
	public bool checkOnStart = true;

	public bool checkOnCollision = true;

	private ScrollingTexture scroller;

	public bool asRigidbody;

	private Rigidbody rb;

	private ComponentsDatabase cdat;

	private void Start()
	{
		cdat = MonoSingleton<ComponentsDatabase>.Instance;
		if (!checkOnStart)
		{
			return;
		}
		if ((bool)cdat && cdat.scrollers.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, LayerMaskDefaults.Get(LMD.Environment));
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (cdat.scrollers.Contains(collider.transform) && collider.transform.TryGetComponent<ScrollingTexture>(out var component))
					{
						component.attachedObjects.Add(base.transform);
					}
				}
			}
		}
		if (!checkOnCollision)
		{
			Object.Destroy(this);
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (!checkOnCollision || !cdat || cdat.scrollers.Count <= 0 || !cdat.scrollers.Contains(col.transform) || !col.transform.TryGetComponent<ScrollingTexture>(out var component))
		{
			return;
		}
		scroller = component;
		if (asRigidbody)
		{
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			component.touchingRbs.Add(rb);
			Vector3 force = component.force;
			if (component.relativeDirection)
			{
				force = new Vector3(component.force.x * component.transform.forward.x, component.force.y * component.transform.forward.y, component.force.z * component.transform.forward.z);
			}
			rb.AddForce(force, ForceMode.VelocityChange);
		}
		else
		{
			component.attachedObjects.Add(base.transform);
		}
	}

	private void OnCollisionExit(Collision col)
	{
		if (!checkOnCollision || !scroller || !(col.transform == scroller.transform))
		{
			return;
		}
		if (asRigidbody)
		{
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			scroller.touchingRbs.Remove(rb);
		}
		else
		{
			scroller.attachedObjects.Remove(base.transform);
		}
	}
}
