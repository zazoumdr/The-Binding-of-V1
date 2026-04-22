using UnityEngine;
using UnityEngine.UI;

public class ScrollUIImage : MonoBehaviour
{
	private RawImage img;

	public float xSpeed;

	public float ySpeed;

	private void Start()
	{
		img = GetComponent<RawImage>();
	}

	private void Update()
	{
		Vector2 vector = img.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.deltaTime;
		while (vector.x > 1f)
		{
			vector.x -= 1f;
		}
		while (vector.x < -1f)
		{
			vector.x += 1f;
		}
		while (vector.y > 1f)
		{
			vector.y -= 1f;
		}
		while (vector.y < -1f)
		{
			vector.y += 1f;
		}
		img.uvRect = new Rect(img.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.deltaTime, img.uvRect.size);
	}
}
