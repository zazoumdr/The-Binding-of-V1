using System.Runtime.CompilerServices;

namespace ULTRAKILL.Portal;

public static class PortalSideExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PortalSide Reverse(this PortalSide side)
	{
		return side ^ (PortalSide)3;
	}

	public static PortalDirection GetDirection(this PortalSide side)
	{
		if (side != PortalSide.Enter)
		{
			return PortalDirection.ExitToEnter;
		}
		return PortalDirection.EnterToExit;
	}

	public static PortalSideFlags ToFlags(this PortalSide side)
	{
		return side switch
		{
			PortalSide.Enter => PortalSideFlags.Enter, 
			PortalSide.Exit => PortalSideFlags.Exit, 
			_ => PortalSideFlags.None, 
		};
	}
}
