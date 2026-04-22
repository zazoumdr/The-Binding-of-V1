using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BloodstainParent : MonoBehaviour
{
	public int parentIndex;

	public Matrix4x4 matrixAtStep;

	private List<int> children = new List<int>();

	private List<int> checkpointChildren = new List<int>();

	private BloodsplatterManager bsm;

	public void OnStep()
	{
		matrixAtStep = GetMatrix();
	}

	public Matrix4x4 GetMatrix()
	{
		base.transform.GetPositionAndRotation(out var position, out var rotation);
		return Matrix4x4.TRS(position, rotation, new Vector3(1f, 1f, 1f));
	}

	private void Start()
	{
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		if (parentIndex == 0)
		{
			parentIndex = bsm.CreateParent(GetMatrix());
			bsm.parents[parentIndex] = this;
		}
		OnStep();
		bsm.reuseParentIndex += OnParentIndexReuse;
		bsm.StainsCleared += OnStainsCleared;
		bsm.PostCollisionStep += OnStep;
	}

	public void OnStainsCleared()
	{
		parentIndex = -1;
		children.Clear();
	}

	public void CreateChild(Vector3 pos, Vector3 norm, bool clipToSurface, bool fromStep)
	{
		if (parentIndex == -1)
		{
			parentIndex = bsm.CreateParent(GetMatrix());
			bsm.parents[parentIndex] = this;
		}
		if (bsm.usedComputeShadersAtStart)
		{
			Vector3 pos2 = base.transform.InverseTransformPoint(pos);
			Vector3 norm2 = base.transform.InverseTransformDirection(norm);
			bsm.CreateBloodstain(pos2, norm2, clipToSurface, this);
		}
		else
		{
			bsm.CreateBloodstain(pos, norm, clipToSurface, this);
		}
	}

	public void AddStain(int index)
	{
		int num = bsm.parentOfStain[index];
		if (num != -1)
		{
			bsm.parents[num].RemoveStain(index);
		}
		bsm.parentOfStain[index] = parentIndex;
		bsm.indexInParentList[index] = children.Count;
		children.Add(index);
	}

	public void RemoveStain(int index)
	{
		int num = bsm.indexInParentList[index];
		int index2 = children.Count - 1;
		int num2 = children[index2];
		children[num] = num2;
		bsm.indexInParentList[num2] = num;
		children.RemoveAt(index2);
		bsm.parentOfStain[index] = -1;
	}

	private void OnParentIndexReuse(int index)
	{
		if (index == parentIndex)
		{
			parentIndex = -1;
			ClearChildren();
		}
	}

	private void Update()
	{
		if (parentIndex != -1 && base.transform.hasChanged)
		{
			bsm.parentMatrices[parentIndex] = GetMatrix();
			base.transform.hasChanged = false;
		}
	}

	private void OnDestroy()
	{
		ClearChildren();
		if ((bool)bsm)
		{
			bsm.reuseParentIndex -= OnParentIndexReuse;
			bsm.PostCollisionStep -= OnStep;
			bsm.StainsCleared -= OnStainsCleared;
		}
	}

	private void OnEnable()
	{
		if (parentIndex >= 0)
		{
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
			if (!(bsm == null) && parentIndex < bsm.parentMatrices.Length && bsm.parentMatrices.IsCreated)
			{
				bsm.parentMatrices[parentIndex] = GetMatrix();
			}
		}
	}

	private void OnDisable()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (parentIndex >= 0 && !(bsm == null) && parentIndex < bsm.parentMatrices.Length && bsm.parentMatrices.IsCreated)
		{
			bsm.parentMatrices[parentIndex] = float4x4.op_Implicit(float4x4.Scale(0f));
		}
	}

	public void ClearChildren()
	{
		if (!(bsm == null) && children.Count != 0)
		{
			for (int i = 0; i < children.Count; i++)
			{
				int num = children[i];
				bsm.parentOfStain[num] = -1;
				bsm.indexInParentList[num] = -1;
				bsm.props[num] = default(BloodsplatterManager.InstanceProperties);
			}
			if (bsm.usedComputeShadersAtStart)
			{
				bsm.instanceBufferDirty = true;
			}
			else
			{
				bsm.meshDirty = true;
			}
			children.Clear();
		}
	}

	public void SaveState()
	{
		checkpointChildren.Clear();
		checkpointChildren.AddRange(children);
	}

	public void LoadState()
	{
		children.Clear();
		children.AddRange(checkpointChildren);
	}
}
