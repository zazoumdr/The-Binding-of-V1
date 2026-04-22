using System;
using Newtonsoft.Json;

[Serializable]
public class SavedGeneric
{
	[NonSerialized]
	public SpawnableObject Spawnable;

	public string ObjectIdentifier;

	public SavedVector3 Position;

	public SavedQuaternion Rotation;

	public SavedVector3 Scale;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public SavedAlterData[] Data;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool DisallowManipulation;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool DisallowFreezing;
}
