using ULTRAKILL.Portal;
using UnityEngine;

public class GasolineProjectile : MonoBehaviour
{
	[SerializeField]
	private GasolineStain stain;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private SphereCollider col;

	private bool hitSomething;

	private void Start()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _) && !TryGetComponent<SimplePortalTraveler>(out var _))
		{
			base.gameObject.AddComponent<SimplePortalTraveler>().SetType(PortalTravellerType.PLAYER_PROJECTILE);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 10 || other.gameObject.layer == 11)
		{
			if (!hitSomething && other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && !component.eid.dead)
			{
				hitSomething = true;
				component.eid.AddFlammable(0.1f);
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (!LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				return;
			}
			Vector3 position = base.transform.position;
			Vector3 vector = -rb.velocity;
			Ray ray = new Ray(base.transform.position - rb.velocity.normalized * Mathf.Max(2.5f, rb.velocity.magnitude * Time.fixedDeltaTime), rb.velocity.normalized);
			if (!other.Raycast(ray, out var hitInfo, 10f) || !LayerMaskDefaults.IsMatchingLayer(hitInfo.transform.gameObject.layer, LMD.Environment))
			{
				return;
			}
			position = hitInfo.point;
			vector = hitInfo.normal;
			bool clipToSurface = true;
			MeshRenderer component2;
			if (!MonoSingleton<PostProcessV2_Handler>.Instance.usedComputeShadersAtStart)
			{
				position += vector * 0.2f;
				clipToSurface = false;
			}
			else if (other.TryGetComponent<MeshRenderer>(out component2))
			{
				Material sharedMaterial = component2.sharedMaterial;
				if (sharedMaterial != null && sharedMaterial.IsKeywordEnabled("VERTEX_DISPLACEMENT"))
				{
					position += vector * 0.2f;
					clipToSurface = false;
				}
			}
			GasolineStain gasolineStain = Object.Instantiate(stain, position, base.transform.rotation);
			Transform obj = gasolineStain.transform;
			obj.forward = vector * -1f;
			obj.Rotate(Vector3.forward * Random.Range(0f, 360f));
			gasolineStain.AttachTo(other, clipToSurface);
			Object.Destroy(base.gameObject);
		}
	}
}
