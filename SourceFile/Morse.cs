using UnityEngine;

public class Morse : MonoBehaviour
{
	[SerializeField]
	private string code;

	[SerializeField]
	private float speed;

	[HideInInspector]
	public int current;

	[SerializeField]
	private UltrakillEvent onDot;

	[SerializeField]
	private UltrakillEvent onDash;

	[SerializeField]
	private UltrakillEvent onSpace;

	private TimeSince timer;

	private void Update()
	{
		if ((float)timer > speed)
		{
			timer = 0f;
			Tick();
		}
	}

	private void Tick()
	{
		if (current >= code.Length)
		{
			current = 0;
			return;
		}
		switch (code[current])
		{
		case '.':
			onDot?.Invoke();
			break;
		case '-':
			onDash?.Invoke();
			break;
		case ' ':
		case '/':
			onSpace?.Invoke();
			break;
		}
		current++;
	}
}
