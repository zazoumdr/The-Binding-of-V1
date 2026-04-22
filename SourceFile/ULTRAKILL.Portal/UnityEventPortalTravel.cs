using System;
using UnityEngine.Events;

namespace ULTRAKILL.Portal;

[Serializable]
public sealed class UnityEventPortalTravel : UnityEvent<IPortalTraveller, PortalTravelDetails>
{
}
