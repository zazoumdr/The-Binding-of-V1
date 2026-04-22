using System.Collections;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class Flight : ICheat
{
	private Rigidbody rigidbody;

	private Transform camera;

	public string LongName => "Flight";

	public string Identifier => "ultrakill.flight";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "flight";

	public bool IsActive { get; private set; }

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.noclip");
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.clash-mode");
		MonoSingleton<NewMovement>.Instance.enabled = false;
		rigidbody = MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>();
		camera = MonoSingleton<CameraController>.Instance.transform;
		IsActive = true;
	}

	public void Disable()
	{
		IsActive = false;
		MonoSingleton<NewMovement>.Instance.enabled = true;
		rigidbody.SetGravityMode(useGravity: true);
	}

	public IEnumerator Coroutine(CheatsManager manager)
	{
		while (IsActive)
		{
			Update();
			yield return null;
		}
	}

	private void Update()
	{
		float num = 1f;
		if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.IsPressed)
		{
			num = 2.5f;
		}
		Vector3 zero = Vector3.zero;
		Vector2 vector = Vector2.ClampMagnitude(MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>(), 1f);
		zero += camera.right * vector.x;
		zero += camera.forward * vector.y;
		if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
		{
			zero += rigidbody.transform.up;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.IsPressed)
		{
			zero += -rigidbody.transform.up;
		}
		rigidbody.velocity = zero * 30f * num;
		MonoSingleton<NewMovement>.Instance.enabled = false;
		rigidbody.isKinematic = false;
		rigidbody.SetGravityMode(useGravity: false);
	}
}
