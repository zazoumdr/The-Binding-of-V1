using ULTRAKILL.Portal;
using UnityEngine;

public class PortalTravelFlagsSetter : MonoBehaviour
{
	public Portal target;

	public PortalTravellerFlags entryTravelFlags;

	public PortalTravellerFlags exitTravelFlags;

	[HideInInspector]
	public bool originalValuesGot;

	public bool updateOriginalValuesWhenActivated;

	[HideInInspector]
	public PortalTravellerFlags origEntryTravelFlags;

	[HideInInspector]
	public PortalTravellerFlags origExitTravelFlags;

	private void Awake()
	{
		if (!originalValuesGot)
		{
			GetValues();
		}
	}

	private void GetValues()
	{
		if (!originalValuesGot || target.entryTravelFlags != entryTravelFlags)
		{
			origEntryTravelFlags = target.entryTravelFlags;
		}
		if (!originalValuesGot || target.exitTravelFlags != exitTravelFlags)
		{
			origExitTravelFlags = target.exitTravelFlags;
		}
		originalValuesGot = true;
	}

	public void Activate()
	{
		if (updateOriginalValuesWhenActivated || !originalValuesGot)
		{
			GetValues();
		}
		target.entryTravelFlags = entryTravelFlags;
		target.exitTravelFlags = exitTravelFlags;
	}

	public void Revert()
	{
		if (!originalValuesGot)
		{
			GetValues();
			return;
		}
		target.entryTravelFlags = origEntryTravelFlags;
		target.exitTravelFlags = origExitTravelFlags;
	}
}
