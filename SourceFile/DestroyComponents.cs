using UnityEngine;

public class DestroyComponents : MonoBehaviour
{
	public Component[] targets;

	public void Activate()
	{
		for (int num = targets.Length - 1; num >= 0; num--)
		{
			if (targets[num] != null)
			{
				Object.Destroy(targets[num]);
			}
		}
	}
}
