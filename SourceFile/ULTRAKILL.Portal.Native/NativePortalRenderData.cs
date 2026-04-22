using System.Runtime.InteropServices;
using Unity.Burst;

namespace ULTRAKILL.Portal.Native;

[BurstCompile]
public struct NativePortalRenderData
{
	[MarshalAs(UnmanagedType.U1)]
	public bool mirror;

	[MarshalAs(UnmanagedType.U1)]
	public bool canSeeItself;

	public int maxRecursions;

	[MarshalAs(UnmanagedType.U1)]
	public bool appearsInRecursions;

	[MarshalAs(UnmanagedType.U1)]
	public bool supportsInfiniteRecursion;

	[MarshalAs(UnmanagedType.U1)]
	public bool overrideFog;

	[MarshalAs(UnmanagedType.U1)]
	public bool canSeePortalLayer;

	public PortalClippingMethod clippingMethod;

	public FogData fogData;

	public PortalSideFlags renderSettings;
}
