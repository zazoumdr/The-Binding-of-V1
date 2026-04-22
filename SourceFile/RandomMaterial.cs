using UnityEngine;

public class RandomMaterial : MonoBehaviour
{
	private Renderer renderer;

	public Material[] materials;

	public int delay = 1;

	public bool instantOnFirstTime;

	private TimeSince previousChange;

	public bool oneTime;

	private bool activated;

	private void Start()
	{
		renderer = GetComponent<Renderer>();
		previousChange = 0f;
		if (!activated && instantOnFirstTime)
		{
			Randomize();
		}
	}

	private void Update()
	{
		if ((!oneTime || !activated) && (float)previousChange > (float)delay)
		{
			previousChange = 0f;
			Randomize();
		}
	}

	public void Randomize()
	{
		activated = true;
		renderer.sharedMaterial = materials[Random.Range(0, materials.Length)];
	}
}
