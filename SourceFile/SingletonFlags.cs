using System;
using System.ComponentModel;

[Flags]
public enum SingletonFlags
{
	None = 0,
	[Description("Instance will not be created automatically.")]
	NoAutoInstance = 1,
	[Description("Automatically created instances will be hidden.")]
	HideAutoInstance = 2,
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This flag is not supported and no longer functions.")]
	[Description("The singleton instance will not be overridden by Awake calls.")]
	NoAwakeInstance = 4,
	[Description("Automatically created instances will survive scene unloads.")]
	PersistAutoInstance = 8,
	[Description("Duplicate instances will be destroyed automatically.")]
	DestroyDuplicates = 0x10
}
