using ULTRAKILL.Portal;

namespace ULTRAKILL.Enemy;

public static class TargetHandleExtensions
{
	public static TargetHandle Then(this TargetHandle handle, PortalScene scene, TargetHandle thenHandle)
	{
		return handle.Then(MonoSingleton<PortalManagerV2>.Instance.Scene, thenHandle);
	}
}
