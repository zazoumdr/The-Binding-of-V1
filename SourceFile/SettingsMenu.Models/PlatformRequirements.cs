using System;

namespace SettingsMenu.Models;

[Serializable]
public class PlatformRequirements
{
	public bool requiresSteam;

	public bool requiresDiscord;

	public bool requiresFileSystemAccess;

	public bool hideInCloudManaged;

	public bool Check()
	{
		_ = requiresSteam;
		_ = requiresDiscord;
		_ = requiresFileSystemAccess;
		if (hideInCloudManaged && IsCloudManagedRelease())
		{
			return false;
		}
		return true;
	}

	public static bool IsCloudManagedRelease()
	{
		return Environment.GetEnvironmentVariable("SOLSTICE_LAUNCH_MODE") == "RELEASE";
	}
}
