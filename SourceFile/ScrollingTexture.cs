using System.Collections.Generic;
using UnityEngine;

public class ScrollingTexture : MonoBehaviour
{
	private static MaterialPropertyBlock _propertyBlock;

	public float scrollSpeedX;

	public float scrollSpeedY;

	private int scrollOffsetID;

	private bool[] usesMasterShader;

	private Dictionary<int, int[]> propertyNames;

	private List<Material> materials = new List<Material>();

	private MeshRenderer mr;

	private Vector2 offset;

	public bool scrollAttachedObjects;

	public Vector3 force;

	public bool relativeDirection;

	public List<Transform> attachedObjects = new List<Transform>();

	[HideInInspector]
	public Bounds bounds;

	[HideInInspector]
	public bool valuesSet;

	[HideInInspector]
	public List<GameObject> cleanUp = new List<GameObject>();

	[HideInInspector]
	public List<WaterDryTracker> specialScrollers = new List<WaterDryTracker>();

	[HideInInspector]
	public List<Rigidbody> touchingRbs = new List<Rigidbody>();

	public BloodstainParent parent;

	private void Start()
	{
		if (scrollAttachedObjects)
		{
			GameObject gameObject = new GameObject("Scrolling Texture Parent");
			parent = gameObject.AddComponent<BloodstainParent>();
		}
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
		}
		mr = GetComponent<MeshRenderer>();
		propertyNames = new Dictionary<int, int[]>();
		mr.GetMaterials(materials);
		usesMasterShader = new bool[materials.Count];
		Shader masterShader = MonoSingleton<DefaultReferenceManager>.Instance.masterShader;
		scrollOffsetID = Shader.PropertyToID("_ScrollOffset");
		for (int i = 0; i < materials.Count; i++)
		{
			Material material = materials[i];
			bool flag = material.shader == masterShader;
			usesMasterShader[i] = flag;
			if (flag)
			{
				material.EnableKeyword("SCROLLING");
			}
		}
		mr.SetMaterials(materials);
		if (!scrollAttachedObjects || valuesSet)
		{
			return;
		}
		valuesSet = true;
		MonoSingleton<ComponentsDatabase>.Instance.scrollers.Add(base.transform);
		Rigidbody component2;
		if (TryGetComponent<Collider>(out var component))
		{
			bounds = component.bounds;
		}
		else if (TryGetComponent<Rigidbody>(out component2))
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			bool flag2 = false;
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].attachedRigidbody == component2)
				{
					if (!flag2)
					{
						bounds = componentsInChildren[j].bounds;
						flag2 = true;
					}
					else
					{
						bounds.Encapsulate(componentsInChildren[j].bounds);
					}
				}
			}
		}
		Invoke("SlowUpdate", 5f);
	}

	private void SlowUpdate()
	{
		foreach (GameObject item in cleanUp)
		{
			Object.Destroy(item);
		}
		cleanUp.Clear();
		Invoke("SlowUpdate", 5f);
	}

	private void OnDestroy()
	{
		if ((bool)parent)
		{
			Object.Destroy(parent.gameObject);
		}
		if (base.gameObject.scene.isLoaded)
		{
			ComponentsDatabase instance = MonoSingleton<ComponentsDatabase>.Instance;
			if ((bool)instance)
			{
				instance.scrollers.Remove(base.transform);
			}
		}
	}

	private void Update()
	{
		offset += new Vector2(scrollSpeedX * Time.deltaTime, scrollSpeedY * Time.deltaTime);
		mr.GetPropertyBlock(_propertyBlock);
		_propertyBlock.SetVector(scrollOffsetID, new Vector4(offset.x, offset.y, 0f, 0f));
		mr.SetPropertyBlock(_propertyBlock);
		Vector3 vector = force;
		if (relativeDirection)
		{
			vector = new Vector3(force.x * base.transform.forward.x, force.y * base.transform.forward.y, force.z * base.transform.forward.z);
		}
		if ((bool)parent)
		{
			parent.transform.position += vector * Time.deltaTime;
		}
		if (!scrollAttachedObjects || attachedObjects.Count <= 0)
		{
			return;
		}
		for (int num = attachedObjects.Count - 1; num >= 0; num--)
		{
			if (attachedObjects[num] != null)
			{
				attachedObjects[num].position = attachedObjects[num].position + vector * Time.deltaTime;
				int num2 = -1;
				if (specialScrollers.Count != 0)
				{
					for (int num3 = specialScrollers.Count - 1; num3 >= 0; num3--)
					{
						if (specialScrollers[num3].transform == null)
						{
							specialScrollers.RemoveAt(num3);
						}
						else if (specialScrollers[num3].transform == attachedObjects[num])
						{
							num2 = num3;
							break;
						}
					}
				}
				if ((num2 < 0 && Vector3.Distance(attachedObjects[num].position, bounds.ClosestPoint(attachedObjects[num].position)) > 1f) || (num2 >= 0 && Vector3.Distance(attachedObjects[num].position + specialScrollers[num2].closestPosition, bounds.ClosestPoint(attachedObjects[num].position + specialScrollers[num2].closestPosition)) > 1f))
				{
					if (num2 >= 0)
					{
						specialScrollers.RemoveAt(num2);
					}
					cleanUp.Add(attachedObjects[num].gameObject);
					attachedObjects[num].gameObject.SetActive(value: false);
					attachedObjects.RemoveAt(num);
				}
			}
			else
			{
				attachedObjects.RemoveAt(num);
			}
		}
	}

	private void FixedUpdate()
	{
		if (touchingRbs.Count <= 0)
		{
			return;
		}
		Vector3 vector = force;
		if (relativeDirection)
		{
			vector = new Vector3(force.x * base.transform.forward.x, force.y * base.transform.forward.y, force.z * base.transform.forward.z);
		}
		for (int num = touchingRbs.Count - 1; num >= 0; num--)
		{
			if (touchingRbs[num] == null)
			{
				touchingRbs.RemoveAt(num);
			}
			else
			{
				touchingRbs[num].AddForce(vector * Time.fixedDeltaTime, ForceMode.VelocityChange);
			}
		}
	}
}
