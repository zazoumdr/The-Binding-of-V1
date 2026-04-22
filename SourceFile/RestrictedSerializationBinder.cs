using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

public sealed class RestrictedSerializationBinder : SerializationBinder
{
	public HashSet<Type> AllowedTypes { get; } = new HashSet<Type>();

	public override Type BindToType(string assemblyName, string typeName)
	{
		string text = Assembly.CreateQualifiedName(assemblyName, typeName);
		foreach (Type allowedType in AllowedTypes)
		{
			if (allowedType.AssemblyQualifiedName == text)
			{
				return allowedType;
			}
		}
		throw new SerializationException("Attempted to serialize restricted type: " + text);
	}
}
