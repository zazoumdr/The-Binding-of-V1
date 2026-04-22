using JigglePhysics;
using UnityEngine;

public class RandomizeWind : MonoBehaviour
{
	public Vector3 randomizeDirectionStrength = Vector3.one;

	private float initialStrength;

	private Vector3 initialDirection;

	private JiggleRigBuilder rig;

	private float waitTime;

	private float elapsedTime;

	private void Start()
	{
		rig = GetComponent<JiggleRigBuilder>();
		initialDirection = rig.wind.normalized;
		initialStrength = rig.wind.magnitude;
	}

	private void Update()
	{
		elapsedTime += Time.deltaTime;
		if (elapsedTime >= waitTime)
		{
			Randomize();
		}
	}

	private void Randomize()
	{
		elapsedTime = 0f;
		waitTime = Random.Range(1f, 5f);
		Vector3 normalized = (Random.rotation.eulerAngles - new Vector3(180f, 180f, 180f)).normalized;
		MonoBehaviour.print(normalized);
		normalized = initialDirection + Vector3.Scale(normalized, randomizeDirectionStrength) * initialStrength;
		rig.wind = normalized;
	}
}
