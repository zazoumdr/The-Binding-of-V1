using System.Linq;
using UnityEngine;

namespace GameConsole;

public class ClipboardStuff : MonoBehaviour
{
	public void TogglePopup()
	{
		base.gameObject.SetActive(!base.gameObject.activeSelf);
	}

	public void CopyToClipboard()
	{
		GUIUtility.systemCopyBuffer = string.Join("\n", MonoSingleton<Console>.Instance.logs.Select((ConsoleLog c) => $"[{c.log.Timestamp:HH:mm:ss.f}] [{c.log.Level}] {c.log.Message}\n{c.log.StackTrace}"));
	}

	public void OpenLogFile()
	{
		string text = Application.persistentDataPath + "/Player.log";
		Application.OpenURL("file://" + text);
	}
}
