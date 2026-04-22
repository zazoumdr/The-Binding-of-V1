using UnityEngine;

public class Landmine : MonoBehaviour
{
	private bool valuesSet;

	private Rigidbody rb;

	private AudioSource aud;

	[SerializeField]
	private GameObject lightCylinder;

	private Light lit;

	private Renderer[] rends;

	private SpriteRenderer sr;

	private MaterialPropertyBlock block;

	[SerializeField]
	private AudioClip activatedBeep;

	[SerializeField]
	private GameObject explosion;

	[SerializeField]
	private GameObject superExplosion;

	[SerializeField]
	private GameObject parryZone;

	private bool activated;

	private bool parried;

	private bool exploded;

	private Vector3 movementDirection;

	public EnemyIdentifier originEnemy;

	private void Start()
	{
		SetValues();
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			rb = GetComponent<Rigidbody>();
			aud = GetComponent<AudioSource>();
			lit = lightCylinder.GetComponentInChildren<Light>();
			rends = GetComponentsInChildren<Renderer>();
			sr = lightCylinder.GetComponentInChildren<SpriteRenderer>();
			block = new MaterialPropertyBlock();
			MonoSingleton<ObjectTracker>.Instance.AddLandmine(this);
		}
	}

	private void OnDestroy()
	{
		if ((bool)MonoSingleton<ObjectTracker>.Instance)
		{
			MonoSingleton<ObjectTracker>.Instance.RemoveLandmine(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!activated && (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject || ((bool)MonoSingleton<PlatformerMovement>.Instance && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject) || (originEnemy != null && originEnemy.target != null && originEnemy.target.enemyIdentifier != null && other.gameObject == originEnemy.target.enemyIdentifier.gameObject) || ((other.gameObject.layer == 10 || other.gameObject.layer == 11) && other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && !component.eid.dead && originEnemy != null && originEnemy.target != null && originEnemy.target.enemyIdentifier != null && originEnemy.target.enemyIdentifier == component.eid)))
		{
			Activate();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!parried || exploded || collision.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			return;
		}
		EnemyIdentifier component = null;
		EnemyIdentifierIdentifier component2 = null;
		if (collision.gameObject.layer == 26 && collision.gameObject.TryGetComponent<ParryHelper>(out var component3) && (bool)component3.target)
		{
			component3.target.TryGetComponent<EnemyIdentifier>(out component);
		}
		if ((bool)component || (collision.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out component2) && (bool)component2.eid) || collision.gameObject.TryGetComponent<EnemyIdentifier>(out component))
		{
			if ((bool)component2 && (bool)component2.eid)
			{
				component = component2.eid;
			}
			if (!component.dead)
			{
				if (component == originEnemy)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(150, "ultrakill.landyours", null, component);
				}
				else
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(75, "ultrakill.serve", null, component);
				}
				if (component.enemyType == EnemyType.Gutterman && component.TryGetComponent<Gutterman>(out var component4) && component4.hasShield)
				{
					component4.ShieldBreak(player: true, flash: false);
				}
			}
		}
		Explode(super: true);
	}

	private void FixedUpdate()
	{
		if (parried)
		{
			rb.velocity = movementDirection * 250f;
		}
	}

	public void Activate(float forceMultiplier = 1f)
	{
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		rb.AddForce(base.transform.up * 20f * forceMultiplier, ForceMode.VelocityChange);
		rb.AddRelativeTorque(Vector3.right * 360f * forceMultiplier, ForceMode.VelocityChange);
		if (!activated)
		{
			activated = true;
			aud.clip = activatedBeep;
			aud.SetPitch(1.5f);
			aud.Play(tracked: true);
			parryZone.SetActive(value: true);
			SetColor(new Color(1f, 0.66f, 0f));
			Invoke("Explode", 1f);
		}
	}

	public void Parry()
	{
		CancelInvoke("Explode");
		parried = true;
		movementDirection = base.transform.forward;
		rb.SetGravityMode(useGravity: true);
		rb.AddRelativeTorque(Vector3.up * 36000f, ForceMode.VelocityChange);
		parryZone.SetActive(value: false);
		SetColor(new Color(0f, 1f, 1f));
		Invoke("Explode", 3f);
	}

	private void Explode()
	{
		Explode(false);
	}

	private void Explode(bool super = false)
	{
		if (!exploded)
		{
			exploded = true;
			Object.Instantiate(super ? superExplosion : explosion, base.transform.position, Quaternion.identity);
			Object.Destroy(base.gameObject);
		}
	}

	public void SetColor(Color newColor)
	{
		if (!valuesSet)
		{
			SetValues();
		}
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			if (!(renderer == sr))
			{
				for (int j = 0; j < renderer.sharedMaterials.Length; j++)
				{
					renderer.GetPropertyBlock(block, j);
					block.SetColor(UKShaderProperties.EmissiveColor, newColor);
					renderer.SetPropertyBlock(block, j);
				}
			}
		}
		sr.color = new Color(newColor.r, newColor.g, newColor.b, sr.color.a);
		lit.color = newColor;
	}
}
