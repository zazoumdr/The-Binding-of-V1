using System.Collections;
using UnityEngine;

public class TerribleTasteBook : MonoBehaviour
{
	public float spinTime = 2f;

	public Coroutine crt;

	public TerribleTasteBook otherSideBook;

	private void Start()
	{
		base.transform.GetChild(0).gameObject.SetActive(value: false);
	}

	public void ActivateBookShelf()
	{
		if (crt == null && otherSideBook.GetComponent<TerribleTasteBook>().crt == null)
		{
			crt = StartCoroutine(SpinShelf());
		}
	}

	private IEnumerator SpinShelf()
	{
		otherSideBook.enabled = false;
		Renderer rend = GetComponent<Renderer>();
		rend.enabled = false;
		MeshCollider col = rend.GetComponent<MeshCollider>();
		col.enabled = false;
		base.transform.GetChild(0).gameObject.SetActive(value: true);
		Transform parent = base.transform.parent;
		Quaternion startRot = parent.rotation;
		Quaternion endRot = parent.rotation * Quaternion.AngleAxis(180f, Vector3.forward);
		float progress = 0f;
		while (progress <= spinTime)
		{
			progress += Time.deltaTime;
			parent.rotation = Quaternion.Slerp(startRot, endRot, progress / spinTime);
			yield return null;
		}
		rend.enabled = true;
		col.enabled = true;
		base.transform.GetChild(0).gameObject.SetActive(value: false);
		yield return null;
		crt = null;
		otherSideBook.enabled = true;
	}
}
