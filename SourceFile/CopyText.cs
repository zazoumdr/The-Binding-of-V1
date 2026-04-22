using TMPro;
using UnityEngine;

public class CopyText : MonoBehaviour
{
	private TMP_Text txt;

	public TMP_Text target;

	private void Start()
	{
		txt = GetComponent<TMP_Text>();
	}

	private void Update()
	{
		txt.text = target.text;
	}
}
