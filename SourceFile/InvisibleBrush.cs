using ULTRAKILL.Cheats;
using UnityEngine;

public class InvisibleBrush : MonoBehaviour
{
	[SerializeField]
	private Renderer renderer;

	private void Update()
	{
		bool anyArmActive = SummonSandboxArm.AnyArmActive;
		if (renderer.enabled != anyArmActive)
		{
			renderer.enabled = anyArmActive;
		}
	}
}
