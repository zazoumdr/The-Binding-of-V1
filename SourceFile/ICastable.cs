public interface ICastable
{
	bool Cast(PortalCastStateV2 state, out PhysicsCastResult result);

	PhysicsCastResult[] CastAll(PortalCastStateV2 state);
}
