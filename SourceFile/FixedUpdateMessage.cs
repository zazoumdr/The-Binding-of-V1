using UnityEngine;

[DisallowMultipleComponent]
public sealed class FixedUpdateMessage : MessageDispatcher
{
	private void FixedUpdate()
	{
		base.Handler.Invoke();
	}
}
