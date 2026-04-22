using System.Collections.Generic;
using UnityEngine;

public class MaliciousFaceCatcher : MonoBehaviour
{
	[HideInInspector]
	public List<Transform> targets = new List<Transform>();

	public void RemoveAll()
	{
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			if (targets[num] != null)
			{
				Object.Destroy(targets[num].gameObject);
			}
		}
		targets.Clear();
	}
}
