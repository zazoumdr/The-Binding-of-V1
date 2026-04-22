using ULTRAKILL.Portal;

namespace ULTRAKILL.Enemy;

public record TargetHandle
{
	public int id => target.Id;

	public readonly ITarget target;

	public readonly PortalHandleSequence portals;

	public readonly int sequenceIndex;

	public TargetHandle(ITarget target, PortalHandleSequence portals, int sequenceIndex)
	{
		this.target = target;
		this.portals = portals;
		this.sequenceIndex = sequenceIndex;
	}

	public TargetHandle Then(PortalHandle handle)
	{
		return new TargetHandle(target, portals.Then(handle), -1);
	}

	public TargetHandle Then(PortalHandleSequence sequence)
	{
		if (sequence.Count == 0)
		{
			return this;
		}
		if (portals.Count == 0)
		{
			return new TargetHandle(target, sequence, -1);
		}
		PortalHandleSequence seq = portals;
		for (int i = 0; i < sequence.Count; i++)
		{
			seq = seq.Then(sequence[i]);
		}
		return new TargetHandle(target, seq, -1);
	}

	public TargetHandle From(PortalHandle handle)
	{
		return new TargetHandle(target, portals.StartFrom(handle), -1);
	}

	public TargetHandle From(PortalHandleSequence sequence)
	{
		if (sequence.Count == 0)
		{
			return this;
		}
		if (portals.Count == 0)
		{
			return new TargetHandle(target, sequence, -1);
		}
		PortalHandleSequence seq = portals;
		for (int i = 0; i < sequence.Count; i++)
		{
			seq = seq.StartFrom(sequence[i]);
		}
		return new TargetHandle(target, seq, -1);
	}

	public TargetHandle(ITarget target, PortalHandleSequence sequence)
	{
		this.target = target;
		portals = sequence;
	}

	public TargetHandle(ITarget target, params PortalHandle[] portals)
	{
		this.target = target;
		if (portals == null || portals.Length == 0)
		{
			this.portals = PortalHandleSequence.Empty;
		}
		else
		{
			this.portals = new PortalHandleSequence(portals);
		}
	}

	public TargetHandle(ITarget target)
	{
		this.target = target;
		portals = PortalHandleSequence.Empty;
	}

	public override int GetHashCode()
	{
		return (29 * 89 + target.Id) * 89 + portals.GetHashCode();
	}

	public virtual bool Equals(TargetHandle other)
	{
		if (other == null)
		{
			return false;
		}
		if (target.Id != other.target.Id)
		{
			return false;
		}
		return portals.Equals(other.portals);
	}
}
