using UnityEngine;

[DisallowMultipleComponent]
public sealed class LateUpdateMessage : MessageDispatcher
{
	private void LateUpdate()
	{
		base.Handler.Invoke();
	}
}
