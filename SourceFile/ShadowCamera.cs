using UnityEngine;
using UnityEngine.Rendering;

public class ShadowCamera
{
	public static (Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) RenderShadowMap(Light light, Bounds groupBounds, ref RenderTexture shadowMap, int shadowIndex)
	{
		Camera camera = new GameObject("Shadow Camera").AddComponent<Camera>();
		camera.transform.SetPositionAndRotation(light.transform.position, light.transform.rotation);
		camera.nearClipPlane = 0.01f;
		camera.cullingMask = light.cullingMask;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.black;
		RenderTexture renderTexture = new RenderTexture(shadowMap.width, shadowMap.height, 24, shadowMap.format);
		if (light.type == LightType.Directional)
		{
			renderTexture.dimension = TextureDimension.Tex2D;
			renderTexture.useMipMap = false;
			var (position, vector) = CalculateCameraParams(light.transform, groupBounds);
			camera.transform.position = position;
			camera.orthographic = true;
			camera.orthographicSize = vector.x;
			camera.nearClipPlane = vector.y;
			camera.farClipPlane = vector.z;
			camera.targetTexture = renderTexture;
			camera.backgroundColor = new Color(-9999f, -9999f, -9999f);
			Shader shader = Shader.Find("ULTRAKILL/Shadowmap_Directional");
			camera.SetReplacementShader(shader, "RenderType");
			camera.Render();
			Graphics.CopyTexture(renderTexture, 0, shadowMap, shadowIndex);
		}
		else
		{
			renderTexture.dimension = TextureDimension.Cube;
			renderTexture.useMipMap = false;
			camera.farClipPlane = light.range * 2f;
			camera.backgroundColor = new Color(9999f, 9999f, 9999f);
			Shader shader2 = Shader.Find("ULTRAKILL/Shadowmap_PointSpot");
			camera.SetReplacementShader(shader2, "RenderType");
			Shader.SetGlobalVector("_LightPos", camera.transform.position);
			camera.RenderToCubemap(renderTexture);
			for (int i = 0; i < 6; i++)
			{
				int dstElement = 6 * shadowIndex + i;
				Graphics.CopyTexture(renderTexture, i, 0, shadowMap, dstElement, 0);
			}
		}
		renderTexture.Release();
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		camera.targetTexture = null;
		camera.ResetReplacementShader();
		Object.DestroyImmediate(camera.gameObject);
		return (viewMatrix: worldToCameraMatrix, projectionMatrix: projectionMatrix);
	}

	public static Bounds CalculateGroupBounds(Renderer[] rends)
	{
		Bounds result = default(Bounds);
		foreach (Renderer renderer in rends)
		{
			result.Encapsulate(renderer.bounds);
		}
		return result;
	}

	public static (Vector3, Vector3) CalculateCameraParams(Transform lightTransform, Bounds groupBounds)
	{
		Vector3[] boundsVertices = GetBoundsVertices(groupBounds);
		Bounds bounds = new Bounds(lightTransform.InverseTransformPoint(boundsVertices[0]), Vector3.zero);
		Vector3[] array = boundsVertices;
		foreach (Vector3 position in array)
		{
			Vector3 point = lightTransform.InverseTransformPoint(position);
			bounds.Encapsulate(point);
		}
		Vector3 item = lightTransform.TransformPoint(bounds.center);
		float x = Mathf.Max(bounds.extents.x, bounds.extents.y);
		float y = 0f - bounds.extents.z;
		float z = bounds.extents.z;
		Vector3 item2 = new Vector3(x, y, z);
		return (item, item2);
	}

	private static Vector3[] GetBoundsVertices(Bounds bounds)
	{
		return new Vector3[8]
		{
			bounds.min,
			new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
			bounds.max
		};
	}
}
