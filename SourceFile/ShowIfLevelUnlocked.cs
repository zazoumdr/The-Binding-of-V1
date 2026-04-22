using UnityEngine;

public class ShowIfLevelUnlocked : MonoBehaviour
{
	public int missionNumber;

	public GameObject[] objectsToHide;

	private void OnEnable()
	{
		RankData rank = GameProgressSaver.GetRank(missionNumber, returnNull: true);
		GameObject[] array = objectsToHide;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(rank != null);
		}
	}
}
