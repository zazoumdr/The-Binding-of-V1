using System.Collections;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class Noclip : ICheat
{
	private Rigidbody rb;

	private KeepInBounds kib;

	private VerticalClippingBlocker vcb;

	private Transform camera;

	public string LongName => "Noclip";

	public string Identifier => "ultrakill.noclip";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "noclip";

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public bool IsActive { get; private set; }

	public void Enable(CheatsManager manager)
	{
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.flight");
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.clash-mode");
		camera = MonoSingleton<CameraController>.Instance.cam.transform;
		rb = MonoSingleton<PlayerTracker>.Instance.GetRigidbody();
		kib = MonoSingleton<NewMovement>.Instance.GetComponent<KeepInBounds>();
		kib.enabled = false;
		vcb = MonoSingleton<NewMovement>.Instance.GetComponent<VerticalClippingBlocker>();
		vcb.enabled = false;
		IsActive = true;
	}

	public void Disable()
	{
		MonoSingleton<NewMovement>.Instance.enabled = true;
		kib.enabled = true;
		vcb.enabled = true;
		rb.isKinematic = false;
		IsActive = false;
	}

	public IEnumerator Coroutine(CheatsManager manager)
	{
		Vector3 lastDirection = Vector3.zero;
		while (IsActive)
		{
			lastDirection = UpdateTick();
			yield return new WaitForEndOfFrame();
		}
		rb.velocity = lastDirection;
	}

	private Vector3 UpdateTick()
	{
		float num = 1f;
		Vector3 vector = Vector3.zero;
		if (MonoSingleton<InputManager>.TryGetInstance(out InputManager instance))
		{
			if (instance.InputSource.Dodge.IsPressed)
			{
				num = 2.5f;
			}
			GameStateManager instance2 = GameStateManager.Instance;
			if ((bool)instance2 && !instance2.IsStateActive("alter-menu"))
			{
				float deltaTime = Time.deltaTime;
				Vector2 vector2 = Vector2.ClampMagnitude(instance.InputSource.Move.ReadValue<Vector2>(), 1f);
				vector = camera.forward * (vector2.y * 40f * num);
				vector += camera.right * (vector2.x * 40f * num);
				if (instance.InputSource.Jump.IsPressed)
				{
					vector += rb.transform.up * 40f * (1f * num);
				}
				if (instance.InputSource.Slide.IsPressed)
				{
					vector += rb.transform.up * -40f * (1f * num);
				}
				rb.position += vector * deltaTime;
			}
			if (MonoSingleton<NewMovement>.TryGetInstance(out NewMovement instance3))
			{
				instance3.enabled = false;
			}
		}
		rb.isKinematic = true;
		return vector;
	}
}
