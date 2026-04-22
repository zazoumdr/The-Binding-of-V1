using System.Collections.Generic;
using UnityEngine;

public class WallCheckGroup : MonoBehaviour
{
	[SerializeField]
	private List<WallCheck> instances;

	public void AddInstance(WallCheck instance)
	{
		if (!(instance == null))
		{
			instances.Add(instance);
		}
	}

	public void RemoveInstance(WallCheck instance)
	{
		if (!(instance == null))
		{
			instances.Remove(instance);
		}
	}

	public bool TryGetActiveInstance(out WallCheck instance)
	{
		int count = instances.Count;
		for (int i = 0; i < count; i++)
		{
			WallCheck wallCheck = instances[i];
			if (IsInstanceValid(wallCheck) && wallCheck.onWall && wallCheck.CheckForCols())
			{
				instance = wallCheck;
				return true;
			}
		}
		instance = null;
		return false;
	}

	public bool CheckForCols()
	{
		int count = instances.Count;
		for (int i = 0; i < count; i++)
		{
			WallCheck wallCheck = instances[i];
			if (IsInstanceValid(wallCheck) && wallCheck.CheckForCols())
			{
				return true;
			}
		}
		return false;
	}

	public bool OnWall()
	{
		int count = instances.Count;
		for (int i = 0; i < count; i++)
		{
			WallCheck wallCheck = instances[i];
			if (IsInstanceValid(wallCheck) && wallCheck.onWall)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckForEnemyCols()
	{
		int count = instances.Count;
		for (int i = 0; i < count; i++)
		{
			WallCheck wallCheck = instances[i];
			if (IsInstanceValid(wallCheck) && wallCheck.CheckForEnemyCols())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInstanceValid(WallCheck instance)
	{
		if (instance != null)
		{
			return instance.isActiveAndEnabled;
		}
		return false;
	}
}
