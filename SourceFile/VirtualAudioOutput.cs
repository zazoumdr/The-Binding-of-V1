using UnityEngine;

public class VirtualAudioOutput : MonoBehaviour
{
	private int _updateIndex;

	public virtual Vector3 GetOutputPosition(AudioListener mainListener, VirtualAudioListener listener, Vector3 position)
	{
		return ((Component)(object)mainListener).transform.TransformPoint(listener.transform.InverseTransformPoint(position));
	}

	public void UpdateCachedValues(int updateIndex)
	{
		if (_updateIndex != updateIndex)
		{
			_updateIndex = updateIndex;
			UpdateCachedValuesCore();
		}
	}

	protected virtual void UpdateCachedValuesCore()
	{
	}
}
