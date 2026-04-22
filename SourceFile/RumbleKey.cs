public class RumbleKey
{
	public string name { get; private set; }

	public int hashKey { get; private set; }

	public RumbleKey(string name)
	{
		this.name = name;
		hashKey = name.GetHashCode();
	}

	public override string ToString()
	{
		return name;
	}

	public override bool Equals(object obj)
	{
		if (obj is RumbleKey rumbleKey)
		{
			return rumbleKey.hashKey == hashKey;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hashKey;
	}
}
