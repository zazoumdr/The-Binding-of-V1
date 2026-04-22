using System.Collections.Generic;
using UnityEngine;

public class PortalLineRenderer
{
	private readonly List<LineRenderer> lines;

	private int numLines = 1;

	private readonly Transform parentTransform;

	public PortalLineRenderer(LineRenderer lineSource)
	{
		parentTransform = lineSource.transform.parent;
		lines = new List<LineRenderer> { lineSource };
	}

	public void SetLines(Vector3 start, Vector3 end, PortalTraversalV2[] traversals)
	{
		numLines = 1;
		if (traversals.Length != 0)
		{
			numLines = traversals.Length + 1;
			if (lines.Count < numLines)
			{
				for (int i = lines.Count; i < numLines; i++)
				{
					LineRenderer item = Object.Instantiate(lines[0], parentTransform);
					lines.Add(item);
				}
			}
			for (int j = 0; j < numLines; j++)
			{
				Vector3 position = ((j == 0) ? start : traversals[j - 1].exitPoint);
				Vector3 position2 = ((j >= traversals.Length) ? end : traversals[j].entrancePoint);
				lines[j].SetPosition(0, position);
				lines[j].SetPosition(1, position2);
			}
		}
		else
		{
			lines[0].SetPosition(0, start);
			lines[0].SetPosition(1, end);
		}
		if (lines.Count > numLines)
		{
			for (int k = numLines; k < lines.Count; k++)
			{
				lines[k].enabled = false;
			}
		}
	}

	public void SetEnabled(bool value)
	{
		for (int i = 0; i < numLines; i++)
		{
			lines[i].enabled = value;
		}
	}

	public void SetGradient(Gradient grad)
	{
		for (int i = 0; i < numLines; i++)
		{
			lines[i].colorGradient = grad;
		}
	}

	public static PortalLineRenderer[] MakeArray(LineRenderer[] lineRenderers)
	{
		PortalLineRenderer[] array = new PortalLineRenderer[lineRenderers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new PortalLineRenderer(lineRenderers[0]);
		}
		return array;
	}
}
