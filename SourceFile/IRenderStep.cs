using UnityEngine;

public interface IRenderStep
{
	void Initialize(Camera mainCamera, RenderTexture mainTexture);

	void Render(Camera mainCamera, RenderTexture mainTexture);

	void Cleanup(Camera mainCamera, RenderTexture mainTexture);
}
