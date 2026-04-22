using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class CustomGravity : MonoBehaviour
{
	private Rigidbody _rigidbody;

	[SerializeField]
	private Vector3 _gravity = Vector3.down;

	[SerializeField]
	private bool _useOriginalDownAsGravity;

	[SerializeField]
	private bool _useGravity;

	public Vector3 gravity
	{
		get
		{
			return _gravity;
		}
		set
		{
			_gravity = value;
		}
	}

	public bool useGravity
	{
		get
		{
			return _useGravity;
		}
		set
		{
			_useGravity = value;
		}
	}

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		if (_useOriginalDownAsGravity)
		{
			_gravity = base.transform.up * -1f * Physics.gravity.magnitude;
		}
	}

	private void OnEnable()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_useGravity = _rigidbody.useGravity;
		_rigidbody.useGravity = false;
	}

	private void OnDisable()
	{
		_rigidbody.useGravity = _useGravity;
	}

	private void FixedUpdate()
	{
		if (_useGravity)
		{
			_rigidbody.AddForce(_gravity, ForceMode.Acceleration);
		}
	}

	private void Reset()
	{
		_gravity = Physics.gravity;
	}
}
