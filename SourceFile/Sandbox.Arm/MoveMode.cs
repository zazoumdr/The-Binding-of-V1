using System;
using System.Collections.Generic;
using plog;
using plog.Models;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sandbox.Arm;

public class MoveMode : ISandboxArmMode, ISandboxArmDebugGUI
{
	public class ManipulatedObject
	{
		public Transform target;

		public SpawnableInstance spawnable;

		public GameObject particles;

		public Rigidbody rigidbody;

		public Collider collider;

		public Vector3 positionOffset;

		public float distance;

		public Vector3 originalRotation;

		public float simpleRotationOffset;

		public Quaternion rotationOffset;

		public ManipulatedObject(PhysicsCastResult hit, SpawnableInstance propOverwrite = null)
		{
			GameObject gameObject = (propOverwrite ? propOverwrite.gameObject : hit.collider.gameObject);
			target = gameObject.transform;
			rigidbody = gameObject.GetComponent<Rigidbody>();
			collider = gameObject.GetComponent<Collider>();
			if (collider == null)
			{
				if (gameObject.TryGetComponent<SandboxPropPart>(out var component))
				{
					collider = component.parent.GetComponent<Collider>();
				}
				if (collider == null)
				{
					collider = gameObject.GetComponentInChildren<Collider>();
				}
			}
			positionOffset = target.position - hit.point;
			CameraController instance = MonoSingleton<CameraController>.Instance;
			distance = Vector3.Distance(instance.transform.position, hit.point);
			originalRotation = target.eulerAngles;
			rotationOffset = Quaternion.Inverse(instance.transform.rotation) * target.rotation;
			simpleRotationOffset = originalRotation.y - instance.rotationY;
			if ((bool)rigidbody)
			{
				rigidbody.isKinematic = true;
			}
			spawnable = (propOverwrite ? propOverwrite : SandboxUtils.GetProp(gameObject));
		}
	}

	private static readonly Logger Log = new Logger("MoveMode");

	private static readonly int Manipulating = Animator.StringToHash("Manipulating");

	private static readonly int Pinched = Animator.StringToHash("Pinched");

	private static readonly int PushZ = Animator.StringToHash("PushZ");

	private static readonly int Crush = Animator.StringToHash("Crush");

	private SandboxArm hostArm;

	private ManipulatedObject manipulatedObject;

	private Vector3 targetPos;

	private Vector3 objectVelocity;

	public string Name => "Move";

	public bool CanOpenMenu => manipulatedObject == null;

	public virtual string Icon => "move";

	public virtual bool Raycast => true;

	public virtual void OnEnable(SandboxArm arm)
	{
		arm.ResetAnimator();
		if (((Behaviour)(object)arm.animator).isActiveAndEnabled)
		{
			arm.animator.SetBool(Manipulating, true);
		}
		hostArm = arm;
	}

	public void OnDisable()
	{
	}

	public void OnDestroy()
	{
	}

	public void Update()
	{
		IntegrityCheck();
		if (manipulatedObject == null)
		{
			return;
		}
		if (ExperimentalArmRotation.Enabled)
		{
			Quaternion identity = Quaternion.identity;
			if (MonoSingleton<InputManager>.Instance.InputSource.NextVariation.IsPressed)
			{
				Vector2 vector = ((InputControl<Vector2>)(object)((Pointer)Mouse.current).delta).ReadValue();
				identity = Quaternion.AngleAxis(vector.x * -0.1f, Vector3.up) * Quaternion.AngleAxis(vector.y * 0.1f, Vector3.right);
				manipulatedObject.rotationOffset = identity * manipulatedObject.rotationOffset;
				MonoSingleton<CameraController>.Instance.activated = false;
			}
			else
			{
				MonoSingleton<CameraController>.Instance.activated = true;
			}
			manipulatedObject.target.rotation = MonoSingleton<CameraController>.Instance.transform.rotation * manipulatedObject.rotationOffset;
		}
		else
		{
			Vector3 vector2 = new Vector3(manipulatedObject.originalRotation.x, MonoSingleton<CameraController>.Instance.rotationY + manipulatedObject.simpleRotationOffset, manipulatedObject.originalRotation.z);
			vector2 = (ULTRAKILL.Cheats.Snapping.SnappingEnabled ? SandboxUtils.SnapRotation(vector2) : vector2);
			manipulatedObject.target.eulerAngles = vector2;
		}
		if (ExperimentalArmRotation.Enabled)
		{
			targetPos = MonoSingleton<CameraController>.Instance.transform.position + MonoSingleton<CameraController>.Instance.transform.forward * manipulatedObject.distance;
			if (ULTRAKILL.Cheats.Snapping.SnappingEnabled)
			{
				targetPos = SandboxUtils.SnapPos(targetPos);
			}
			Vector3 vector3 = targetPos - manipulatedObject.target.position;
			manipulatedObject.particles.transform.position = manipulatedObject.collider.bounds.center;
			manipulatedObject.target.position += vector3 * 15.5f * Time.deltaTime;
			objectVelocity = vector3;
		}
		else
		{
			Vector3 vector4 = new Vector3(manipulatedObject.originalRotation.x, MonoSingleton<CameraController>.Instance.rotationY + manipulatedObject.simpleRotationOffset, manipulatedObject.originalRotation.z);
			vector4 = (ULTRAKILL.Cheats.Snapping.SnappingEnabled ? SandboxUtils.SnapRotation(vector4) : vector4);
			manipulatedObject.target.eulerAngles = vector4;
			Vector3 vector5 = Quaternion.Euler(0f, 0f - (manipulatedObject.originalRotation.y - vector4.y), 0f) * (ULTRAKILL.Cheats.Snapping.SnappingEnabled ? SandboxUtils.SnapPos(manipulatedObject.positionOffset) : manipulatedObject.positionOffset);
			Vector3 vector6 = manipulatedObject.target.position - vector5;
			Vector3 vector7 = (targetPos = MonoSingleton<CameraController>.Instance.transform.position + MonoSingleton<CameraController>.Instance.transform.forward * manipulatedObject.distance);
			if (ULTRAKILL.Cheats.Snapping.SnappingEnabled)
			{
				vector7 = SandboxUtils.SnapPos(vector7);
			}
			Vector3 vector8 = vector7 - vector6;
			manipulatedObject.particles.transform.position = manipulatedObject.collider.bounds.center;
			manipulatedObject.target.position += vector8 * 15.5f * Time.deltaTime;
			objectVelocity = vector8;
		}
		float y = ((InputControl<Vector2>)(object)Mouse.current.scroll).ReadValue().y;
		hostArm.animator.SetFloat(PushZ, y);
		manipulatedObject.distance += Mathf.Clamp(manipulatedObject.distance, 1f, 10f) / 10f * y * 0.05f;
		manipulatedObject.distance = Mathf.Max(0f, manipulatedObject.distance);
	}

	public void FixedUpdate()
	{
	}

	public void OnPrimaryDown()
	{
		if (manipulatedObject == null && hostArm.hitSomething)
		{
			SpawnableInstance prop = SandboxUtils.GetProp(hostArm.hit.collider.gameObject);
			if ((bool)prop)
			{
				prop.Pause();
				MonoSingleton<GunControl>.Instance.activated = false;
				manipulatedObject = new ManipulatedObject(hostArm.hit, prop)
				{
					particles = UnityEngine.Object.Instantiate(hostArm.manipulateEffect)
				};
				hostArm.animator.SetBool(Pinched, true);
			}
		}
	}

	public void OnPrimaryUp()
	{
		if (manipulatedObject != null)
		{
			Vector3 vector = objectVelocity;
			manipulatedObject.particles.transform.position = manipulatedObject.collider.bounds.center;
			manipulatedObject.target.position += vector * 15.5f * Time.deltaTime;
			if (SandboxArmDebug.DebugActive)
			{
				Log.Info($"Target Position: {targetPos}\n" + $"Object's Position: {manipulatedObject.target.position}\n" + $"Position Delta: {vector}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			ReleaseManipulatedObject(vector * 6.5f);
		}
	}

	public bool OnGUI()
	{
		if (manipulatedObject == null)
		{
			if (hostArm.hitSomething && hostArm.hit.collider != null)
			{
				GUILayout.Label("Hit: " + hostArm.hit.collider.name, Array.Empty<GUILayoutOption>());
				GUILayout.Space(8f);
				int layer = hostArm.hit.collider.gameObject.layer;
				GUILayout.Label("Layer: " + layer + " - " + LayerMask.LayerToName(layer), Array.Empty<GUILayoutOption>());
				GUILayout.Label("Tag: " + hostArm.hit.collider.tag, Array.Empty<GUILayoutOption>());
			}
			else
			{
				GUILayout.Label("No raycast hit.", Array.Empty<GUILayoutOption>());
			}
			return true;
		}
		GUILayout.Label("Moving: " + manipulatedObject.target.name, Array.Empty<GUILayoutOption>());
		GUILayout.Space(8f);
		Vector3 vector = targetPos;
		GUILayout.Label("Computed Target Point: " + vector.ToString(), Array.Empty<GUILayoutOption>());
		GUILayout.Label("Object's (Pivot) Position " + manipulatedObject.target.position.ToString(), Array.Empty<GUILayoutOption>());
		vector = objectVelocity;
		GUILayout.Label("Velocity: " + vector.ToString(), Array.Empty<GUILayoutOption>());
		return true;
	}

	public void OnSecondaryDown()
	{
		if (manipulatedObject != null && (!(manipulatedObject.spawnable != null) || !manipulatedObject.spawnable.disallowFreezing))
		{
			if ((bool)manipulatedObject.particles)
			{
				UnityEngine.Object.Destroy(manipulatedObject.particles);
			}
			MonoSingleton<GunControl>.Instance.activated = true;
			hostArm.animator.SetBool(Pinched, false);
			hostArm.animator.SetTrigger(Crush);
			SpawnableInstance component = manipulatedObject.target.GetComponent<SpawnableInstance>();
			component.Pause();
			if (manipulatedObject.target.CompareTag("Untagged"))
			{
				manipulatedObject.target.tag = "Floor";
			}
			if (MonoSingleton<SandboxNavmesh>.Instance != null)
			{
				MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty(component);
			}
			hostArm.freezeSound.SetPitch(UnityEngine.Random.Range(1f, 1.05f));
			hostArm.freezeSound.Play(tracked: true);
			GameObject gameObject = new GameObject("Ghost Effect Wrapper");
			gameObject.transform.position = manipulatedObject.collider.bounds.center;
			GameObject gameObject2 = gameObject;
			SandboxUtils.StripForPreview(UnityEngine.Object.Instantiate(manipulatedObject.target, gameObject2.transform, worldPositionStays: true), hostArm.previewMaterial);
			gameObject2.gameObject.AddComponent<SandboxGhostEffect>();
			MonoSingleton<CameraController>.Instance.activated = true;
			manipulatedObject = null;
		}
	}

	public void OnSecondaryUp()
	{
	}

	private void IntegrityCheck()
	{
		if (manipulatedObject != null && (!(manipulatedObject.target != null) || !(manipulatedObject.collider != null)))
		{
			Log.Warning("Integrity check failed, releasing manipulated object", (IEnumerable<Tag>)null, (string)null, (object)null);
			ReleaseManipulatedObject(Vector3.zero);
		}
	}

	private void ReleaseManipulatedObject(Vector3 velocity, Quaternion? deltaRot = null)
	{
		UnityEngine.Object.Destroy(manipulatedObject.particles);
		MonoSingleton<GunControl>.Instance.activated = true;
		hostArm.animator.SetBool(Pinched, false);
		MonoSingleton<CameraController>.Instance.activated = true;
		if (manipulatedObject.target != null && manipulatedObject.target.TryGetComponent<SpawnableInstance>(out var component))
		{
			if (!component.alwaysFrozen)
			{
				component.Resume();
			}
			else
			{
				component.frozen = true;
			}
		}
		if ((bool)manipulatedObject.rigidbody && (!manipulatedObject.spawnable.sourceObject || !manipulatedObject.spawnable.sourceObject.alwaysKinematic))
		{
			manipulatedObject.rigidbody.isKinematic = false;
			manipulatedObject.rigidbody.velocity = velocity;
			manipulatedObject.rigidbody.angularVelocity = deltaRot?.eulerAngles ?? Vector3.zero;
		}
		manipulatedObject = null;
	}
}
