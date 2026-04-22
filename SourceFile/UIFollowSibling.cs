using UnityEngine;

public class UIFollowSibling : MonoBehaviour
{
	[SerializeField]
	private RectTransform target;

	[SerializeField]
	private Vector3 offset = Vector3.zero;

	private RectTransform rectTransform;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		if (target == null)
		{
			Debug.LogWarning("UIFollowSibling: Target is not assigned.");
		}
	}

	private void Update()
	{
		if (target != null)
		{
			rectTransform.position = target.position + offset;
			rectTransform.rotation = target.rotation;
			rectTransform.localScale = target.localScale;
		}
	}
}
