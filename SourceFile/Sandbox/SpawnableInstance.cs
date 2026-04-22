using System;
using System.Collections.Generic;
using System.Linq;
using plog;
using plog.Models;
using UnityEngine;

namespace Sandbox;

public class SpawnableInstance : MonoBehaviour
{
	private static readonly Logger Log = new Logger("SpawnableInstance");

	public SpawnableObject sourceObject;

	[NonSerialized]
	public GameObject attachedParticles;

	[NonSerialized]
	public Collider collider;

	[NonSerialized]
	public bool alwaysFrozen;

	[NonSerialized]
	public Rigidbody rigidbody;

	public bool frozen;

	public bool disallowManipulation;

	public bool disallowFreezing;

	public bool uniformSize
	{
		get
		{
			Vector3 vector = normalizedSize;
			if (vector.x == vector.y)
			{
				return vector.y == vector.z;
			}
			return false;
		}
	}

	public Vector3 defaultSize { get; private set; }

	public Vector3 normalizedSize => new Vector3(base.transform.localScale.x / defaultSize.x, base.transform.localScale.y / defaultSize.y, base.transform.localScale.z / defaultSize.z);

	public virtual void Awake()
	{
		defaultSize = base.transform.localScale;
		rigidbody = GetComponent<Rigidbody>();
		if (collider == null)
		{
			collider = GetComponent<Collider>();
		}
		if (collider == null)
		{
			collider = base.transform.GetComponentInChildren<Collider>();
		}
		SandboxPropPart[] componentsInChildren = GetComponentsInChildren<SandboxPropPart>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].parent = this;
		}
	}

	public virtual void SetSize(Vector3 size)
	{
		base.transform.localScale = new Vector3(size.x * defaultSize.x, size.y * defaultSize.y, size.z * defaultSize.z);
	}

	public void SetSizeUniform(float size)
	{
		SetSize(Vector3.one * size);
	}

	public void BaseSave(ref SavedGeneric saveObject)
	{
		if (saveObject != null)
		{
			saveObject.Spawnable = sourceObject;
			saveObject.ObjectIdentifier = sourceObject.identifier;
			saveObject.Position = new SavedVector3(base.transform.position);
			saveObject.Rotation = new SavedQuaternion(base.transform.rotation);
			saveObject.Scale = new SavedVector3(normalizedSize);
			if (saveObject is SavedPhysical savedPhysical)
			{
				savedPhysical.Kinematic = frozen;
			}
			saveObject.DisallowManipulation = disallowManipulation;
			saveObject.DisallowFreezing = disallowFreezing;
		}
		else
		{
			saveObject = new SavedGeneric
			{
				Spawnable = sourceObject,
				ObjectIdentifier = sourceObject.identifier,
				Position = new SavedVector3(base.transform.position),
				Rotation = new SavedQuaternion(base.transform.rotation),
				Scale = new SavedVector3(normalizedSize),
				DisallowManipulation = disallowManipulation,
				DisallowFreezing = disallowFreezing
			};
		}
		IAlter[] array = (from c in GetComponentsInChildren<IAlter>()
			where c.alterKey != null
			select c).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		saveObject.Data = new SavedAlterData[array.Length];
		for (int num = 0; num < array.Length; num++)
		{
			IAlter alter = array[num];
			List<SavedAlterOption> list = new List<SavedAlterOption>();
			if (alter is IAlterOptions<bool> alterOptions)
			{
				if (alterOptions.options != null)
				{
					list.AddRange(from b in alterOptions.options
						where b.key != null
						select new SavedAlterOption
						{
							BoolValue = b.value,
							Key = b.key
						});
				}
				Log.Fine("Saving <b>bool</b> data for <b>" + alter.alterKey + "</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
				Log.Fine((alterOptions.options == null) ? "No options available!" : ("Bool Options: " + string.Join(", ", alterOptions.options.Select((AlterOption<bool> o) => o.key ?? "(missing key!)"))), (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			if (alter is IAlterOptions<float> alterOptions2)
			{
				if (alterOptions2.options != null)
				{
					list.AddRange(from b in alterOptions2.options
						where b.key != null
						select new SavedAlterOption
						{
							FloatValue = b.value,
							Key = b.key
						});
				}
				Log.Fine("Saving <b>float</b> data for <b>" + alter.alterKey + "</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
				Log.Fine((alterOptions2.options == null) ? "No options available!" : ("Float Options: " + string.Join(", ", alterOptions2.options.Select((AlterOption<float> o) => o.key ?? "(missing key!)"))), (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			if (alter is IAlterOptions<Vector3> alterOptions3)
			{
				if (alterOptions3.options != null)
				{
					list.AddRange(from b in alterOptions3.options
						where b.key != null
						select new SavedAlterOption
						{
							VectorData = b.value,
							Key = b.key
						});
				}
				if (Debug.isDebugBuild)
				{
					Log.Fine("Saving <b>vector</b> data for <b>" + alter.alterKey + "</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
					Log.Fine((alterOptions3.options == null) ? "No options available!" : ("Vector Options: " + string.Join(", ", alterOptions3.options.Select((AlterOption<Vector3> o) => o.key ?? "(missing key!)"))), (IEnumerable<Tag>)null, (string)null, (object)null);
				}
			}
			if (alter is IAlterOptions<int> alterOptions4)
			{
				if (alterOptions4.options != null)
				{
					list.AddRange(from b in alterOptions4.options
						where b.key != null
						select new SavedAlterOption
						{
							IntValue = b.value,
							Key = b.key
						});
				}
				Log.Fine("Saving <b>float</b> data for <b>" + alter.alterKey + "</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
				Log.Fine((alterOptions4.options == null) ? "No options available!" : ("Float Options: " + string.Join(", ", alterOptions4.options.Select((AlterOption<int> o) => o.key ?? "(missing key!)"))), (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			saveObject.Data[num] = new SavedAlterData
			{
				Key = array[num].alterKey,
				Options = list.ToArray()
			};
		}
	}

	public virtual void Pause(bool freeze = true)
	{
		if (freeze)
		{
			frozen = true;
		}
	}

	public virtual void Resume()
	{
		frozen = false;
	}

	public void ApplyAlterOptions(IEnumerable<AlterOption> requestedOptions)
	{
		IAlterOptions<bool>[] array = null;
		IAlterOptions<int>[] array2 = null;
		IAlterOptions<float>[] array3 = null;
		IAlterOptions<Vector3>[] array4 = null;
		foreach (AlterOption requestedOption in requestedOptions)
		{
			if (requestedOption.useBool)
			{
				if (array == null)
				{
					array = GetComponentsInChildren<IAlterOptions<bool>>(includeInactive: true);
				}
				IAlterOptions<bool>[] array5 = array;
				for (int i = 0; i < array5.Length; i++)
				{
					AlterOption<bool>[] options = array5[i].options;
					foreach (AlterOption<bool> alterOption in options)
					{
						if (!(alterOption.key != requestedOption.targetKey))
						{
							alterOption.callback(requestedOption.boolValue);
						}
					}
				}
			}
			if (requestedOption.useInt)
			{
				if (array2 == null)
				{
					array2 = GetComponentsInChildren<IAlterOptions<int>>(includeInactive: true);
				}
				IAlterOptions<int>[] array6 = array2;
				for (int i = 0; i < array6.Length; i++)
				{
					AlterOption<int>[] options2 = array6[i].options;
					foreach (AlterOption<int> alterOption2 in options2)
					{
						if (!(alterOption2.key != requestedOption.targetKey))
						{
							alterOption2.callback(requestedOption.intValue);
						}
					}
				}
			}
			if (requestedOption.useFloat)
			{
				if (array3 == null)
				{
					array3 = GetComponentsInChildren<IAlterOptions<float>>(includeInactive: true);
				}
				IAlterOptions<float>[] array7 = array3;
				for (int i = 0; i < array7.Length; i++)
				{
					AlterOption<float>[] options3 = array7[i].options;
					foreach (AlterOption<float> alterOption3 in options3)
					{
						if (!(alterOption3.key != requestedOption.targetKey))
						{
							alterOption3.callback(requestedOption.floatValue);
						}
					}
				}
			}
			if (!requestedOption.useVector)
			{
				continue;
			}
			if (array4 == null)
			{
				array4 = GetComponentsInChildren<IAlterOptions<Vector3>>(includeInactive: true);
			}
			IAlterOptions<Vector3>[] array8 = array4;
			for (int i = 0; i < array8.Length; i++)
			{
				AlterOption<Vector3>[] options4 = array8[i].options;
				foreach (AlterOption<Vector3> alterOption4 in options4)
				{
					if (!(alterOption4.key != requestedOption.targetKey))
					{
						alterOption4.callback(requestedOption.vectorValue);
					}
				}
			}
		}
	}
}
