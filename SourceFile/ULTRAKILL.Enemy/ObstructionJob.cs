using System.Runtime.InteropServices;
using Unity.Jobs;

namespace ULTRAKILL.Enemy;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ObstructionJob : IJobFor
{
	public void Execute(int index)
	{
	}
}
