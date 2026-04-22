using UnityEngine;

[DisallowMultipleComponent]
public sealed class ParticleSystemStoppedMessage : MessageDispatcher
{
	private void OnParticleSystemStopped()
	{
		base.Handler.Invoke();
	}
}
