using System.Text;
using UnityEngine;

public static class TransformExtensions
{
	public static void GetScenePath(this Transform transform, StringBuilder builder)
	{
		if (transform.parent == null)
		{
			builder.Append('/');
			builder.Append(transform.name);
		}
		else
		{
			transform.parent.GetScenePath(builder);
			builder.Append('/');
			builder.Append(transform.name);
		}
	}

	public static string GetScenePath(this Transform transform)
	{
		if (transform.parent == null)
		{
			return "/" + transform.name;
		}
		return transform.parent.GetScenePath() + "/" + transform.name;
	}
}
