using UnityEngine;

public class AddStylePoints : MonoBehaviour
{
	public string styleName;

	public int points;

	private void Start()
	{
		Activate();
	}

	public void Activate()
	{
		MonoSingleton<StyleHUD>.Instance.AddPoints(points, styleName);
	}
}
