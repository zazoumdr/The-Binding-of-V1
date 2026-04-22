using Sandbox;
using UnityEngine;

public class BrushBlock : SandboxProp, IAlter, IAlterOptions<Vector3>
{
	public Vector3 DataSize;

	public BlockType Type;

	public BoxCollider OverrideCollider;

	public BoxCollider WaterTrigger;

	public string alterKey => "block";

	public string alterCategoryName => "Material Block";

	public AlterOption<Vector3>[] options => new AlterOption<Vector3>[1]
	{
		new AlterOption<Vector3>
		{
			name = "Size",
			key = null,
			value = DataSize,
			callback = delegate(Vector3 value)
			{
				DataSize = value;
				float b = 10000f;
				DataSize.x = Mathf.Min(DataSize.x, b);
				DataSize.y = Mathf.Min(DataSize.y, b);
				DataSize.z = Mathf.Min(DataSize.z, b);
				RegenerateMesh();
			}
		}
	};

	public SavedBlock SaveBrushBlock()
	{
		SavedBlock obj = new SavedBlock
		{
			BlockSize = new SavedVector3(DataSize),
			BlockType = Type
		};
		SavedGeneric saveObject = obj;
		BaseSave(ref saveObject);
		return obj;
	}

	public void RegenerateMesh()
	{
		Mesh mesh = SandboxUtils.GenerateProceduralMesh(DataSize, simple: false);
		GetComponent<MeshFilter>().mesh = mesh;
		BoxCollider obj = (OverrideCollider ? OverrideCollider : GetComponent<BoxCollider>());
		obj.size = DataSize;
		obj.center = obj.size / 2f;
		if ((bool)WaterTrigger)
		{
			WaterTrigger.size = DataSize;
			WaterTrigger.center = WaterTrigger.size / 2f;
		}
		if (MonoSingleton<SandboxNavmesh>.Instance != null && frozen)
		{
			MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty(this);
		}
	}
}
