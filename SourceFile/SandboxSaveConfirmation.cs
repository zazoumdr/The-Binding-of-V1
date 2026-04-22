using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxSaveConfirmation : MonoSingleton<SandboxSaveConfirmation>
{
	[SerializeField]
	private GameObject saveConfirmationDialog;

	[SerializeField]
	private GameObject blocker;

	public void ConfirmSave()
	{
		MonoSingleton<SandboxSaver>.Instance.QuickSave();
		saveConfirmationDialog.SetActive(value: false);
		blocker.SetActive(value: false);
	}

	public void DisplayDialog()
	{
		saveConfirmationDialog.SetActive(value: true);
		blocker.SetActive(value: true);
	}
}
