using UnityEngine;

public class FakeFallZone : MonoBehaviour
{
	private int requests;

	public float heightControlAmount = 20f;

	[SerializeField]
	private GameObject freeFallHud;

	public RectTransform freeFallMarker;

	public float markerHeight;

	private void OnDisable()
	{
		if (requests != 0)
		{
			MonoSingleton<NewMovement>.Instance.fakeFallRequests -= requests;
			requests = 0;
			MonoSingleton<NewMovement>.Instance.fakeFallZones.Remove(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(other.gameObject != MonoSingleton<NewMovement>.Instance.gameObject))
		{
			if (requests == 0)
			{
				MonoSingleton<NewMovement>.Instance.fakeFallZones.Add(this);
			}
			requests++;
			MonoSingleton<NewMovement>.Instance.fakeFallRequests++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!(other.gameObject != MonoSingleton<NewMovement>.Instance.gameObject))
		{
			requests--;
			if (requests == 0)
			{
				MonoSingleton<NewMovement>.Instance.fakeFallZones.Remove(this);
			}
			MonoSingleton<NewMovement>.Instance.fakeFallRequests--;
		}
	}

	private void Update()
	{
		if (!(freeFallHud == null))
		{
			if (requests == 0)
			{
				freeFallHud.SetActive(value: false);
				return;
			}
			freeFallHud.SetActive(value: true);
			float num = Mathf.Clamp((MonoSingleton<NewMovement>.Instance.rb.position.y - base.transform.position.y) / heightControlAmount, -1f, 1f);
			freeFallMarker.anchoredPosition = new Vector2(freeFallMarker.anchoredPosition.x, 0f + markerHeight * num);
		}
	}

	private void FixedUpdate()
	{
		if (requests != 0 && !MonoSingleton<NewMovement>.Instance.gc.onGround && !MonoSingleton<NewMovement>.Instance.TeleportFixedFramesPending && (!MonoSingleton<HookArm>.Instance || !MonoSingleton<HookArm>.Instance.beingPulled))
		{
			float num = 0f;
			if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
			{
				num += heightControlAmount;
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Slide.IsPressed)
			{
				num -= heightControlAmount;
			}
			NewMovement instance = MonoSingleton<NewMovement>.Instance;
			bool num2 = instance.ridingRocket != null;
			Transform transform = (num2 ? instance.ridingRocket.transform : instance.transform);
			Rigidbody rigidbody = (num2 ? instance.ridingRocket.rb : instance.rb);
			Vector3 vector = base.transform.position + Vector3.up * num - transform.position;
			vector -= rigidbody.velocity / 2f;
			rigidbody.AddForce(Vector3.up * vector.y, ForceMode.VelocityChange);
		}
	}
}
