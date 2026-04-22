using UnityEngine;

public class EventOnSceneName : MonoBehaviour
{
	public string sceneName;

	public bool emitRevertOnSceneMismatch;

	public UltrakillEvent onSceneName;

	private void OnEnable()
	{
		if (SceneHelper.CurrentScene == sceneName)
		{
			onSceneName.Invoke();
		}
		else if (emitRevertOnSceneMismatch)
		{
			onSceneName.Revert();
		}
	}
}
