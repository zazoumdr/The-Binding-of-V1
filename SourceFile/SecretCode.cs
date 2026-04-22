using UnityEngine;

public class SecretCode : MonoBehaviour
{
	public string code;

	public bool revertEventOnFailure;

	public int startingPoint;

	[HideInInspector]
	public int currentPoint;

	public UltrakillEvent onSuccess;

	private void Start()
	{
		currentPoint = startingPoint;
	}

	public void Input(string entry)
	{
		if (entry.Length > 0)
		{
			if (entry.Length > 1)
			{
				Debug.LogWarning("Only the first character of a SecretCode input will be considered.");
			}
			Input(entry[0]);
		}
	}

	public void Input(char entry)
	{
		if (currentPoint >= code.Length || code[currentPoint] != entry)
		{
			currentPoint = 0;
			if (revertEventOnFailure)
			{
				onSuccess?.Revert();
			}
			return;
		}
		currentPoint++;
		if (currentPoint == code.Length)
		{
			onSuccess?.Invoke();
			currentPoint = 0;
		}
	}

	public void Reset()
	{
		currentPoint = 0;
		if (revertEventOnFailure)
		{
			onSuccess?.Revert();
		}
	}
}
