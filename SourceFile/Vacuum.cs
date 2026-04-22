using System;
using UnityEngine;

public class Vacuum : MonoBehaviour
{
	private readonly struct StuckObject
	{
		public readonly Rigidbody rigidbody;

		private readonly float _maxAngularVelocity;

		private readonly RigidbodyInterpolation _interpolation;

		public void UndoPropertyModifications()
		{
			if (!(rigidbody == null))
			{
				rigidbody.maxAngularVelocity = _maxAngularVelocity;
				rigidbody.interpolation = _interpolation;
			}
		}

		public StuckObject(Rigidbody instance)
		{
			rigidbody = instance;
			_maxAngularVelocity = instance.maxAngularVelocity;
			_interpolation = instance.interpolation;
			instance.maxAngularVelocity = float.PositiveInfinity;
			instance.interpolation = RigidbodyInterpolation.Interpolate;
		}
	}

	[SerializeField]
	private float _suckStrength = 10f;

	[SerializeField]
	private float _stuckDistance = 0.5f;

	[SerializeField]
	private Transform _suckPoint;

	[SerializeField]
	private BoxCollider _suckBox;

	[SerializeField]
	private AudioSource _consumeSound;

	[SerializeField]
	private AudioSource _suckSound;

	[SerializeField]
	private ParticleSystem _suckSystem;

	[SerializeField]
	private ParticleSystem _blowSystem;

	private ArraySegment<Collider> _colliders = new ArraySegment<Collider>(new Collider[256], 0, 0);

	private bool _isSucking;

	private bool _isBlowing;

	private StuckObject _stuckObject;

	private Vector3 _lastCameraRotation;

	private bool musicStarted;

	[SerializeField]
	private GameObject music;

	[Header("Sound Effects")]
	[SerializeField]
	private AudioClip suckStartSound;

	[SerializeField]
	private AudioClip suckLoopSound;

	[SerializeField]
	private AudioClip suckStopSound;

	[SerializeField]
	private AudioClip suckStuckSound;

	[SerializeField]
	private AudioClip suckStuckLoopSound;

	[SerializeField]
	private AudioClip blowStartSound;

	[SerializeField]
	private AudioClip blowLoopSound;

	[SerializeField]
	private AudioClip blowStopSound;

	private void Update()
	{
		if (MonoSingleton<GunControl>.Instance.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			UpdateInput();
		}
		if (!_isSucking && !_isBlowing)
		{
			return;
		}
		if (!_suckSound.isPlaying || _suckSound.time > _suckSound.clip.length - 0.1f)
		{
			if (_stuckObject.rigidbody != null)
			{
				_suckSound.clip = suckStuckLoopSound;
			}
			else if (_isBlowing)
			{
				_suckSound.clip = blowLoopSound;
			}
			else
			{
				_suckSound.clip = suckLoopSound;
			}
			_suckSound.loop = true;
			_suckSound.Play();
		}
		float f = (float)((double)Time.time % 6.283185);
		_suckSound.SetPitch(1.1f + Mathf.Sin(f) * 0.025f);
		SuckObjects();
	}

	private void FixedUpdate()
	{
		if (_isSucking)
		{
			UpdateStuckObject();
		}
	}

	private void UpdateInput()
	{
		if (!_isBlowing && MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
		{
			StartBlowing();
		}
		else if (_isBlowing && !MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
		{
			StopBlowing();
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !_isBlowing && !_isSucking)
		{
			StartVacuuming();
		}
		else if (_isSucking && !MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			StopVacuuming();
		}
	}

	private void UpdateStuckObject()
	{
		if (!(_stuckObject.rigidbody == null))
		{
			Vector3 vector = _suckPoint.position - _stuckObject.rigidbody.worldCenterOfMass;
			_stuckObject.rigidbody.velocity = vector / Time.fixedDeltaTime;
			float num = Camera.main.transform.eulerAngles.y - _lastCameraRotation.y;
			num *= MathF.PI / 180f;
			_stuckObject.rigidbody.angularVelocity = new Vector3(0f, num / Time.fixedDeltaTime, 0f);
			_lastCameraRotation = Camera.main.transform.eulerAngles;
		}
	}

	private void UpdateColliders()
	{
		int i = _colliders.Offset;
		for (int num = i + _colliders.Count; i < num; i++)
		{
			Collider collider = _colliders.Array[i];
			if (!(collider == null) && !(collider.attachedRigidbody == null) && collider.attachedRigidbody.TryGetComponent<Cork>(out var component))
			{
				component.insideSuckZone = false;
			}
		}
		Vector3 center = _suckBox.transform.TransformPoint(_suckBox.center);
		Vector3 halfExtents = Vector3.Scale(_suckBox.size, _suckBox.transform.lossyScale) * 0.5f;
		_colliders = new ArraySegment<Collider>(_colliders.Array, 0, Physics.OverlapBoxNonAlloc(center, halfExtents, _colliders.Array, _suckBox.transform.rotation));
		int j = _colliders.Offset;
		for (int num2 = j + _colliders.Count; j < num2; j++)
		{
			Collider collider2 = _colliders.Array[j];
			if (!(collider2 == null) && !(collider2.attachedRigidbody == null) && collider2.attachedRigidbody.TryGetComponent<Cork>(out var component2))
			{
				component2.insideSuckZone = true;
			}
		}
	}

	private void SuckObjects()
	{
		if ((!_isSucking && !_isBlowing) || _stuckObject.rigidbody != null)
		{
			return;
		}
		UpdateColliders();
		int i = _colliders.Offset;
		for (int num = i + _colliders.Count; i < num; i++)
		{
			Collider collider = _colliders.Array[i];
			if (collider == null || collider.attachedRigidbody == null || collider.attachedRigidbody.TryGetComponent<NewMovement>(out var _))
			{
				continue;
			}
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (_isSucking && attachedRigidbody.TryGetComponent<Cork>(out var component2))
			{
				component2.StartWiggle();
			}
			if (_isSucking && collider.TryGetComponent<TerribleTasteBook>(out var component3))
			{
				component3.ActivateBookShelf();
			}
			GhostDrone component4 = null;
			if (_isSucking && attachedRigidbody.TryGetComponent<GhostDrone>(out component4))
			{
				Vector3 vector = _suckPoint.position - attachedRigidbody.position;
				Vector3 vacuumVelocity = vector.normalized * Mathf.Max(1f / vector.sqrMagnitude, 2f);
				component4.vacuumVelocity = vacuumVelocity;
			}
			if (_isSucking)
			{
				attachedRigidbody.velocity = Vector3.Normalize(_suckPoint.position - attachedRigidbody.worldCenterOfMass) * _suckStrength;
			}
			else
			{
				attachedRigidbody.velocity = MonoSingleton<CameraController>.Instance.transform.forward.normalized * _suckStrength * 2f;
			}
			if (_isBlowing || Vector3.Distance(attachedRigidbody.worldCenterOfMass, _suckPoint.position) >= _stuckDistance)
			{
				continue;
			}
			if (!attachedRigidbody.TryGetComponent<GoreSplatter>(out var component5))
			{
				if ((bool)component4)
				{
					component4.KillGhost();
					continue;
				}
				SetStuckObject(attachedRigidbody);
				break;
			}
			if (!musicStarted)
			{
				if ((bool)music)
				{
					music.SetActive(value: true);
				}
				musicStarted = true;
			}
			_consumeSound.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
			_consumeSound.PlayOneShot(_consumeSound.clip, tracked: true);
			component5.Repool();
		}
	}

	private void StopCorkPull()
	{
		int i = _colliders.Offset;
		for (int num = i + _colliders.Count; i < num; i++)
		{
			Collider collider = _colliders.Array[i];
			if (!(collider == null) && !(collider.attachedRigidbody == null) && collider.attachedRigidbody.TryGetComponent<Cork>(out var component))
			{
				component.StopWiggle();
			}
		}
	}

	private void StartVacuuming()
	{
		if (_isBlowing)
		{
			StopBlowing();
		}
		_suckSystem.Play();
		_suckSound.clip = suckStartSound;
		_suckSound.loop = false;
		_suckSound.Play();
		_isSucking = true;
	}

	private void StopVacuuming()
	{
		_suckSystem.Stop();
		_suckSound.clip = suckStopSound;
		_suckSound.loop = false;
		_suckSound.Play();
		_isSucking = false;
		StopCorkPull();
		SetStuckObject(null);
	}

	private void StartBlowing()
	{
		if (_isSucking)
		{
			StopVacuuming();
		}
		_blowSystem.Play();
		_suckSound.clip = blowStartSound;
		_suckSound.loop = false;
		_suckSound.Play();
		_isBlowing = true;
		StopCorkPull();
		SetStuckObject(null);
	}

	private void StopBlowing()
	{
		_blowSystem.Stop();
		_suckSound.clip = blowStopSound;
		_suckSound.loop = false;
		_suckSound.Play();
		_isBlowing = false;
	}

	private void SetStuckObject(Rigidbody rigidbody)
	{
		if (_stuckObject.rigidbody != null)
		{
			_stuckObject.UndoPropertyModifications();
			_stuckObject.rigidbody.velocity = MonoSingleton<NewMovement>.Instance.rb.velocity;
			_stuckObject.rigidbody.angularVelocity = Vector3.zero;
			EnemyIdentifierIdentifier component2;
			if (_stuckObject.rigidbody.TryGetComponent<GoreSplatter>(out var component))
			{
				component.bloodAbsorberCount--;
			}
			else if (_stuckObject.rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out component2))
			{
				component2.bloodAbsorberCount--;
			}
		}
		if (rigidbody == null)
		{
			_stuckObject = default(StuckObject);
			return;
		}
		EnemyIdentifierIdentifier component4;
		if (rigidbody.TryGetComponent<GoreSplatter>(out var component3))
		{
			component3.bloodAbsorberCount++;
		}
		else if (rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out component4))
		{
			component4.bloodAbsorberCount++;
		}
		if (!musicStarted)
		{
			if ((bool)music)
			{
				music.SetActive(value: true);
			}
			musicStarted = true;
		}
		_suckSystem.Stop();
		_suckSound.loop = false;
		_suckSound.clip = suckStuckSound;
		_suckSound.Play();
		_stuckObject = new StuckObject(rigidbody);
		StopCorkPull();
	}
}
