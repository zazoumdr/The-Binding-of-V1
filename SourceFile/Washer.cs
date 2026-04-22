using System.Collections.Generic;
using UnityEngine;

public class Washer : MonoBehaviour
{
	private bool isSpraying;

	public ParticleSystem part;

	public List<ParticleCollisionEvent> collisionEvents;

	private InputManager inputManager;

	private AudioSource aud;

	[SerializeField]
	private AudioClip click;

	[SerializeField]
	private AudioClip triggerOn;

	[SerializeField]
	private AudioClip triggerOff;

	private ShapeModule shapeModule;

	private MainModule mainModule;

	[SerializeField]
	private GameObject[] nozzles;

	private bool musicStarted;

	[SerializeField]
	private GameObject music;

	private Vector3 defaultSprayPos;

	private Quaternion defaultSprayRot;

	private int nozzleMode;

	public CorrectCameraView correctCameraView;

	private void Start()
	{
		collisionEvents = new List<ParticleCollisionEvent>();
		defaultSprayPos = correctCameraView.transform.localPosition;
		defaultSprayRot = correctCameraView.transform.localRotation;
	}

	private void OnEnable()
	{
		part = GetComponent<ParticleSystem>();
		part.Stop();
		aud = GetComponent<AudioSource>();
		aud.Stop();
		inputManager = MonoSingleton<InputManager>.Instance;
	}

	private void Update()
	{
		Transform transform = MonoSingleton<CameraController>.Instance.transform;
		if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 50f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			if (hitInfo.distance < 2.25f)
			{
				base.transform.position = transform.position;
				base.transform.rotation = transform.rotation;
				correctCameraView.canModifyTarget = false;
			}
			else
			{
				correctCameraView.canModifyTarget = true;
			}
		}
		if (MonoSingleton<GunControl>.Instance.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (inputManager.InputSource.Fire1.IsPressed && !isSpraying)
			{
				StartWashing();
			}
			else if (!inputManager.InputSource.Fire1.IsPressed && isSpraying)
			{
				StopWashing();
			}
			if (inputManager.InputSource.Fire2.WasPerformedThisFrame)
			{
				SwitchNozzle();
			}
		}
		float f = (float)((double)Time.time % 6.283185);
		aud.SetPitch(((nozzleMode == 2) ? 2.1f : 1.1f) + Mathf.Sin(f) * 0.025f);
	}

	private void SwitchNozzle()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		aud.SetPitch(Random.Range(0.9f, 1.1f));
		aud.PlayOneShot(click, tracked: true);
		nozzleMode = (nozzleMode + 1) % 3;
		for (int i = 0; i < nozzles.Length; i++)
		{
			nozzles[i].SetActive(i == nozzleMode);
		}
		shapeModule = part.shape;
		mainModule = part.main;
		EmissionModule emission = part.emission;
		if (nozzleMode == 0)
		{
			((MainModule)(ref mainModule)).startLifetime = MinMaxCurve.op_Implicit(0.5f);
			((MainModule)(ref mainModule)).startSpeed = MinMaxCurve.op_Implicit(100f);
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(1000f);
			((ShapeModule)(ref shapeModule)).angle = 11f;
			((ShapeModule)(ref shapeModule)).rotation = new Vector3(0f, 0f, 0f);
			((ShapeModule)(ref shapeModule)).scale = new Vector3(0.1f, 1f, 1f);
		}
		if (nozzleMode == 1)
		{
			((MainModule)(ref mainModule)).startLifetime = MinMaxCurve.op_Implicit(0.5f);
			((MainModule)(ref mainModule)).startSpeed = MinMaxCurve.op_Implicit(100f);
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(1000f);
			((ShapeModule)(ref shapeModule)).angle = 11f;
			((ShapeModule)(ref shapeModule)).rotation = new Vector3(0f, 0f, 90f);
			((ShapeModule)(ref shapeModule)).scale = new Vector3(0.1f, 1f, 1f);
		}
		if (nozzleMode == 2)
		{
			((MainModule)(ref mainModule)).startLifetime = MinMaxCurve.op_Implicit(1.2f);
			((MainModule)(ref mainModule)).startSpeed = MinMaxCurve.op_Implicit(100f);
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(700f);
			((ShapeModule)(ref shapeModule)).angle = 0.75f;
			((ShapeModule)(ref shapeModule)).scale = Vector3.one;
		}
	}

	private void StartWashing()
	{
		aud.SetPitch(Random.Range(0.9f, 1.1f));
		aud.PlayOneShot(triggerOn, tracked: true);
		isSpraying = true;
		part.Play();
		aud.Play();
	}

	private void StopWashing()
	{
		isSpraying = false;
		part.Stop();
		aud.Stop();
		aud.SetPitch(Random.Range(0.9f, 1.1f));
		aud.PlayOneShot(triggerOff, tracked: true);
	}

	private void OnParticleCollision(GameObject other)
	{
		BloodAbsorberChild component2;
		if (other.TryGetComponent<BloodAbsorber>(out var component))
		{
			if (!musicStarted)
			{
				if ((bool)music)
				{
					music.SetActive(value: true);
				}
				musicStarted = true;
			}
			Vector3 position = ((Component)(object)part).transform.position;
			ParticlePhysicsExtensions.GetCollisionEvents(part, other, collisionEvents);
			component.ProcessWasherSpray(ref collisionEvents, position, null);
		}
		else if (other.TryGetComponent<BloodAbsorberChild>(out component2))
		{
			if (!musicStarted)
			{
				if ((bool)music)
				{
					music.SetActive(value: true);
				}
				musicStarted = true;
			}
			Vector3 position2 = ((Component)(object)part).transform.position;
			ParticlePhysicsExtensions.GetCollisionEvents(part, other, collisionEvents);
			component2.ProcessWasherSpray(ref collisionEvents, position2);
			if (other.TryGetComponent<SpinFromForce>(out var component3))
			{
				component3.AddSpin(ref collisionEvents);
			}
		}
		GameObject gameObject = other.gameObject;
		if (gameObject.layer == 12 && gameObject.TryGetComponent<EnemyIdentifier>(out var component4) && component4.enemyType == EnemyType.Streetcleaner && !component4.dead)
		{
			component4.InstaKill();
		}
	}
}
