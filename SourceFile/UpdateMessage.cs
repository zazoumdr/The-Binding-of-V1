using UnityEngine;

[DisallowMultipleComponent]
public sealed class UpdateMessage : MessageDispatcher
{
	private void Update()
	{
		base.Handler.Invoke();
	}
}
