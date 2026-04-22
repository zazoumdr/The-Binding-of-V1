using ULTRAKILL.Portal;
using UnityEngine;

namespace DefaultNamespace;

public class PortalCrossTest : MonoBehaviour
{
	public Transform a;

	public Transform b;

	public PortalScene scene;

	private void Awake()
	{
		scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
	}

	private void Update()
	{
	}
}
