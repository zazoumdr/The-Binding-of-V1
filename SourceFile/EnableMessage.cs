using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnableMessage : MessageDispatcher
{
	private void OnEnable()
	{
		base.Handler.Invoke();
	}
}
