using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CausticVolumeManager : MonoSingleton<CausticVolumeManager>
{
	private struct CausticData
	{
		public Vector4 position_nearRadius;

		public Vector4 color_farRadius;
	}

	public List<CausticVolume> causticVolumes = new List<CausticVolume>();

	private List<ObjectBoundsToShader> objects = new List<ObjectBoundsToShader>();

	private List<CausticData> causticDataArray = new List<CausticData>();

	private ComputeBuffer causticVolumeBuffer;

	public bool isDirty;

	private void OnValidate()
	{
		isDirty = true;
	}

	private void OnDestroy()
	{
		ReleaseBuffer();
	}

	private void ReleaseBuffer()
	{
		Shader.SetGlobalBuffer("_CausticVolumeData", (ComputeBuffer)null);
		causticVolumeBuffer?.Dispose();
	}

	public void AddObject(ObjectBoundsToShader rendObj)
	{
		if (!objects.Contains(rendObj))
		{
			objects.Add(rendObj);
		}
		isDirty = true;
	}

	public void RemoveObject(ObjectBoundsToShader rendObj)
	{
		objects.Remove(rendObj);
		isDirty = true;
	}

	public void AddVolume(CausticVolume volume)
	{
		if (!causticVolumes.Contains(volume))
		{
			causticVolumes.Add(volume);
		}
		isDirty = true;
	}

	public void RemoveVolume(CausticVolume volume)
	{
		causticVolumes.Remove(volume);
		isDirty = true;
	}

	private void LateUpdate()
	{
		if (isDirty)
		{
			UpdateCausticData();
			isDirty = false;
		}
	}

	private void Start()
	{
		if (causticVolumeBuffer == null || !causticVolumeBuffer.IsValid() || causticVolumeBuffer.count < causticVolumes.Count)
		{
			causticVolumeBuffer?.Release();
			causticVolumeBuffer = new ComputeBuffer(1, 32);
		}
		Shader.SetGlobalBuffer("_CausticVolumeData", causticVolumeBuffer);
	}

	private void UpdateCausticData()
	{
		for (int num = causticVolumes.Count - 1; num >= 0; num--)
		{
			if (causticVolumes[num] == null || !causticVolumes[num].isActiveAndEnabled)
			{
				RemoveVolume(causticVolumes[num]);
			}
		}
		foreach (ObjectBoundsToShader @object in objects)
		{
			@object.UpdateRendererBounds();
		}
		causticVolumeBuffer?.Release();
		causticDataArray.Clear();
		CausticData item = default(CausticData);
		foreach (CausticVolume causticVolume in causticVolumes)
		{
			Vector4 position_nearRadius = causticVolume.transform.position;
			position_nearRadius.w = causticVolume.nearRadius;
			item.position_nearRadius = position_nearRadius;
			Vector4 color_farRadius = causticVolume.color * causticVolume.intensity;
			color_farRadius.w = causticVolume.farRadius;
			item.color_farRadius = color_farRadius;
			causticDataArray.Add(item);
		}
		if (causticVolumeBuffer == null || !causticVolumeBuffer.IsValid() || causticVolumeBuffer.count < causticVolumes.Count)
		{
			causticVolumeBuffer?.Release();
			causticVolumeBuffer = new ComputeBuffer(Mathf.Max(1, causticVolumes.Count), 32);
		}
		causticVolumeBuffer.SetData(causticDataArray);
		Shader.SetGlobalBuffer("_CausticVolumeData", causticVolumeBuffer);
	}
}
