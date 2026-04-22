using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class LeaderboardFixDialog : MonoSingleton<LeaderboardFixDialog>
{
	[SerializeField]
	private BasicConfirmationDialog dialog;

	public void ShowDialog()
	{
		dialog.ShowDialog();
	}
}
