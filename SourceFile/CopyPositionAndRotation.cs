using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(int.MinValue)]
public sealed class CopyPositionAndRotation : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("target")]
	private Transform m_Target;

	[SerializeField]
	[FormerlySerializedAs("copyRotation")]
	private bool m_CopyRotation = true;

	[SerializeField]
	[FormerlySerializedAs("copyPosition")]
	private bool m_CopyPosition = true;

	public bool useRelative;

	private Vector3 _initialPositionOffset;

	private Quaternion _initialRotationOffset;

	public Transform target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public bool copyRotation
	{
		get
		{
			return m_CopyRotation;
		}
		set
		{
			m_CopyRotation = value;
		}
	}

	public bool copyPosition
	{
		get
		{
			return m_CopyPosition;
		}
		set
		{
			m_CopyPosition = value;
		}
	}

	private void Start()
	{
		if (!(m_Target == null))
		{
			_initialPositionOffset = base.transform.position - m_Target.position;
			_initialRotationOffset = Quaternion.Inverse(m_Target.rotation) * base.transform.rotation;
		}
	}

	private void LateUpdate()
	{
		Tick();
	}

	public void Tick()
	{
		if (m_Target == null)
		{
			return;
		}
		if (useRelative)
		{
			if (m_CopyPosition)
			{
				base.transform.position = m_Target.position + _initialPositionOffset;
			}
			if (m_CopyRotation)
			{
				base.transform.rotation = m_Target.rotation * _initialRotationOffset;
			}
		}
		else
		{
			if (m_CopyRotation)
			{
				base.transform.rotation = m_Target.rotation;
			}
			if (m_CopyPosition)
			{
				base.transform.position = m_Target.position;
			}
		}
	}
}
