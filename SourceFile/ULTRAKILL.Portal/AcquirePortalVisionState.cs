using ULTRAKILL.Enemy;

namespace ULTRAKILL.Portal;

public class AcquirePortalVisionState
{
	public TargetData target { get; private set; }

	public AcquirePortalVisionType type { get; private set; }

	public bool started { get; private set; }

	public bool completed { get; private set; }

	public AcquirePortalVisionState()
	{
		Reset();
	}

	public void SetType(AcquirePortalVisionType type)
	{
		this.type = type;
	}

	public void Start()
	{
		started = true;
	}

	public void Complete()
	{
		completed = true;
	}

	public void Reset(AcquirePortalVisionType type = AcquirePortalVisionType.NONE, TargetData target = default(TargetData))
	{
		this.type = type;
		this.target = target;
		started = false;
		completed = false;
	}
}
