using UnityEngine;

public static class PolygonRenderer
{
	public static void DrawPortal(Vector3[] clippedPoly, Material debugMat)
	{
		Mesh mesh = CreateMeshFromPolygon(clippedPoly);
		debugMat.SetPass(0);
		Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
	}

	private static Mesh CreateMeshFromPolygon(Vector3[] vertices)
	{
		Mesh mesh = new Mesh();
		int num = vertices.Length;
		int[] array = new int[(num - 2) * 3];
		int num2 = 0;
		for (int i = 1; i < num - 1; i++)
		{
			array[num2] = 0;
			array[num2 + 1] = i;
			array[num2 + 2] = i + 1;
			num2 += 3;
		}
		mesh.vertices = vertices;
		mesh.triangles = array;
		return mesh;
	}
}
