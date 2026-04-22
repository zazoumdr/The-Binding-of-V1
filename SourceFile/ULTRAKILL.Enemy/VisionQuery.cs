namespace ULTRAKILL.Enemy;

public class VisionQuery
{
	public delegate bool TargetPredicate(TargetDataRef target);

	public TargetPredicate predicate;

	public string Name { get; }

	public VisionQuery(string name, TargetPredicate predicate)
	{
		Name = name;
		this.predicate = predicate;
	}
}
