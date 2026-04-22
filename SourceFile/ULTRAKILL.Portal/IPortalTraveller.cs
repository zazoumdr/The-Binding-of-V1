using UnityEngine;

namespace ULTRAKILL.Portal;

public interface IPortalTraveller
{
	int id { get; }

	PortalTravellerType travellerType { get; }

	Vector3 travellerPosition { get; }

	Vector3 travellerVelocity { get; }

	bool? OnTravel(PortalTravelDetails details);

	void OnTeleportBlocked(PortalTravelDetails details);
}
