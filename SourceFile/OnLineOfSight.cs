using UnityEngine;

public class OnLineOfSight : MonoBehaviour
{
	public UltrakillEvent onLineOfSight;

	private Collider col;

	public bool oneTime;

	[HideInInspector]
	public bool activated;

	[HideInInspector]
	public bool beenActivatedOnce;

	public bool disableOnExit;

	private void Awake()
	{
		col = GetComponent<Collider>();
	}

	private void Update()
	{
		if (oneTime && beenActivatedOnce)
		{
			return;
		}
		Vector3 defaultPos = MonoSingleton<CameraController>.Instance.GetDefaultPos();
		if (col == null)
		{
			if (!Physics.Raycast(base.transform.position, defaultPos - base.transform.position, Vector3.Distance(defaultPos, base.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				Activate();
			}
			else if (activated)
			{
				Deactivate();
			}
			return;
		}
		bool flag = false;
		Vector3 vector = base.transform.position;
		for (int i = 0; i < 9; i++)
		{
			switch (i)
			{
			case 0:
				vector = base.transform.position;
				break;
			case 1:
				vector = col.bounds.min;
				break;
			case 2:
				vector = new Vector3(col.bounds.min.x, col.bounds.min.y, col.bounds.max.z);
				break;
			case 3:
				vector = new Vector3(col.bounds.min.x, col.bounds.max.y, col.bounds.min.z);
				break;
			case 4:
				vector = new Vector3(col.bounds.max.x, col.bounds.min.y, col.bounds.min.z);
				break;
			case 5:
				vector = new Vector3(col.bounds.min.x, col.bounds.max.y, col.bounds.max.z);
				break;
			case 6:
				vector = new Vector3(col.bounds.max.x, col.bounds.min.y, col.bounds.max.z);
				break;
			case 7:
				vector = new Vector3(col.bounds.max.x, col.bounds.max.y, col.bounds.min.z);
				break;
			case 8:
				vector = col.bounds.max;
				break;
			}
			if (!Physics.Raycast(vector, defaultPos - vector, Vector3.Distance(defaultPos, vector), LayerMaskDefaults.Get(LMD.Environment)))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Activate();
		}
		else if (activated)
		{
			Deactivate();
		}
	}

	private void Activate()
	{
		if (!activated && (!oneTime || !beenActivatedOnce))
		{
			activated = true;
			beenActivatedOnce = true;
			onLineOfSight.Invoke();
		}
	}

	private void Deactivate()
	{
		if (activated)
		{
			activated = false;
			onLineOfSight.Revert();
		}
	}
}
