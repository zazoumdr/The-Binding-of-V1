using System.Collections.Generic;

namespace ULTRAKILL.Portal;

public static class PortalHandleSequenceExtensions
{
	public static PortalHandleSequence Then(this PortalHandleSequence seq, PortalHandle thenHandle)
	{
		return seq.Then(MonoSingleton<PortalManagerV2>.Instance.Scene, thenHandle);
	}

	public static PortalHandleSequence StartFrom(this PortalHandleSequence seq, PortalHandle fromHandle)
	{
		return seq.StartFrom(MonoSingleton<PortalManagerV2>.Instance.Scene, fromHandle);
	}

	public static PortalHandleSequence ToPortalHandleSequence(this IEnumerable<PortalHandle> handles)
	{
		if (handles == null)
		{
			return PortalHandleSequence.Empty;
		}
		List<PortalHandle> list = new List<PortalHandle>();
		foreach (PortalHandle handle in handles)
		{
			list.Add(handle);
		}
		return new PortalHandleSequence(list.ToArray());
	}

	public static bool AllHasFlag(this PortalHandleSequence seq, PortalTravellerFlags flag)
	{
		for (int i = 0; i < seq.Count; i++)
		{
			PortalHandle handle = seq[i];
			if (!PortalUtils.GetPortalObject(handle).GetTravelFlags(handle.side).HasFlag(flag))
			{
				return false;
			}
		}
		return true;
	}
}
