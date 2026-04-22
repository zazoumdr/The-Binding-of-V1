using UnityEngine;

public class VirtueInsignia : MonoBehaviour
{
	public EnemyTarget target;

	public GameObject explosion;

	public int damage;

	private bool hasHitTarget;

	private float offset;

	private SpriteRenderer[] sprites;

	private bool activating;

	private float activationTime;

	private float currentDistance;

	public float windUpSpeedMultiplier = 1f;

	public float explosionLength;

	public int charges;

	private float explosionWidth;

	private AudioSource explAud;

	private Light lit;

	[HideInInspector]
	public Enemy parentEnemy;

	[HideInInspector]
	public Drone parentDrone;

	[HideInInspector]
	public Transform otherParent;

	public bool hadParent;

	public bool predictive;

	public bool noTracking;

	public Sprite predictiveVersion;

	public Sprite multiVersion;

	private void Start()
	{
		offset = Random.Range(-0.1f, 0.1f);
		sprites = GetComponentsInChildren<SpriteRenderer>();
		explosionWidth = explosion.transform.localScale.x;
		if ((bool)parentEnemy)
		{
			if (parentEnemy.TryGetComponent<Drone>(out var component))
			{
				component.childVi.Add(this);
			}
			hadParent = true;
		}
		else if ((bool)parentDrone)
		{
			parentDrone.childVi.Add(this);
			hadParent = true;
		}
		if ((bool)otherParent)
		{
			hadParent = true;
		}
		for (int i = 1; i < sprites.Length; i++)
		{
			sprites[i].gameObject.SetActive(value: false);
			sprites[i].transform.localPosition = Vector3.zero;
		}
		if ((charges > 1 && multiVersion != null && !predictive) || (predictiveVersion != null && predictive))
		{
			SpriteRenderer[] array = sprites;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (predictive)
				{
					spriteRenderer.sprite = predictiveVersion;
				}
				else if (charges > 1)
				{
					spriteRenderer.sprite = multiVersion;
				}
			}
		}
		lit = GetComponent<Light>();
		if (!noTracking)
		{
			Invoke("Activating", 2f / windUpSpeedMultiplier);
		}
		else
		{
			Activating();
		}
	}

	private void Update()
	{
		if (!activating)
		{
			if (activationTime < 1f)
			{
				if (hadParent && !parentEnemy && !parentDrone && !otherParent)
				{
					Object.Destroy(base.gameObject);
				}
				base.transform.SetPositionAndRotation(Vector3.MoveTowards(base.transform.position, target.position + Vector3.up * offset, Time.deltaTime * 50f + Time.deltaTime * Vector3.Distance(base.transform.position, target.position) * 100f), Quaternion.FromToRotation(Vector3.up, target.targetTransform.up) * Quaternion.AngleAxis(Time.timeSinceLevelLoad * 180f, Vector3.up));
				return;
			}
			explosionLength = Mathf.MoveTowards(explosionLength, 0f, Time.deltaTime);
			if (explosionLength <= 1f)
			{
				explosion.transform.localScale = new Vector3(explosionLength * explosionWidth, explosion.transform.localScale.y, explosionLength * explosionWidth);
				explAud.SetPitch(explosionLength);
				if (explosionLength <= 0f)
				{
					Object.Destroy(base.gameObject);
				}
			}
			return;
		}
		if (noTracking && hadParent && !parentEnemy && !parentDrone && (!otherParent || !otherParent.gameObject.activeSelf))
		{
			Object.Destroy(base.gameObject);
		}
		base.transform.Rotate(Vector3.up, Time.deltaTime * 720f, Space.Self);
		activationTime = Mathf.MoveTowards(activationTime, 1f, Time.deltaTime * windUpSpeedMultiplier);
		currentDistance = Mathf.MoveTowards(currentDistance, 1f, Time.deltaTime * (windUpSpeedMultiplier * 2f * (1f - currentDistance)));
		for (int i = 1; i < sprites.Length; i++)
		{
			if (i % 2 == 0)
			{
				sprites[i].transform.localPosition = 3f * Mathf.Lerp(0f, i / 2 * -1, currentDistance) * Vector3.up;
			}
			else
			{
				sprites[i].transform.localPosition = 3f * Mathf.Lerp(0f, (i + 1) / 2, currentDistance) * Vector3.up;
			}
		}
		if (activationTime >= 1f)
		{
			activating = false;
			explosionLength += 1f;
			Explode();
		}
	}

	private void Activating()
	{
		if (!noTracking && charges > 1)
		{
			charges--;
			Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity).GetComponent<VirtueInsignia>().target = target;
		}
		activating = true;
		if (predictive)
		{
			if (target != null)
			{
				float num = 1f / windUpSpeedMultiplier;
				Vector3 velocity = target.GetVelocity();
				if (target.isPlayer)
				{
					if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
					{
						base.transform.position = target.PredictTargetPosition(num);
					}
					else
					{
						base.transform.position = target.position + Vector3.up * offset + (num - (base.transform.localScale.x - 1f) / 20f * num) * 16.45f * new Vector3(velocity.x, 0f, velocity.z).normalized;
					}
				}
				else
				{
					base.transform.position = target.position + Vector3.up * offset + new Vector3(velocity.x, 0f, velocity.z) * (num - (base.transform.localScale.x - 1f) / 20f * num);
				}
			}
			else
			{
				base.transform.position = target.position + Vector3.up * offset;
			}
		}
		for (int i = 1; i < sprites.Length; i++)
		{
			sprites[i].gameObject.SetActive(value: true);
			sprites[i].transform.localPosition = Vector3.zero;
		}
		currentDistance = 0f;
		activationTime = 0f;
	}

	private void Explode()
	{
		if (noTracking && charges > 1 && (!hadParent || (bool)parentEnemy || (bool)parentDrone || ((bool)otherParent && otherParent.gameObject.activeSelf)))
		{
			charges--;
			VirtueInsignia component = Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity).GetComponent<VirtueInsignia>();
			component.target = target;
			component.hadParent = hadParent;
			if ((bool)parentEnemy)
			{
				component.parentEnemy = parentEnemy;
			}
			if ((bool)parentDrone)
			{
				component.parentDrone = parentDrone;
			}
			if ((bool)otherParent)
			{
				component.otherParent = otherParent;
			}
		}
		SpriteRenderer[] array = sprites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		explAud = explosion.GetComponent<AudioSource>();
		explosion.SetActive(value: true);
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
		if ((bool)lit)
		{
			lit.enabled = false;
		}
		if ((bool)parentEnemy)
		{
			if (parentEnemy.TryGetComponent<Drone>(out var component2))
			{
				component2.childVi.Remove(this);
			}
		}
		else if ((bool)parentDrone)
		{
			parentDrone.childVi.Remove(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (target != null && !hasHitTarget && (!target.isPlayer || other.gameObject.CompareTag("Player")))
		{
			if (target.isPlayer)
			{
				hasHitTarget = true;
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
				{
					MonoSingleton<PlatformerMovement>.Instance.Burn();
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.LaunchFromPoint(MonoSingleton<NewMovement>.Instance.transform.position, 200f, 5f);
					MonoSingleton<NewMovement>.Instance.GetHurt(damage, invincible: true);
				}
			}
			else
			{
				EnemyIdentifier enemyIdentifier = other.GetComponent<EnemyIdentifier>();
				if (enemyIdentifier == null)
				{
					EnemyIdentifierIdentifier component = other.GetComponent<EnemyIdentifierIdentifier>();
					if (component != null)
					{
						enemyIdentifier = component.eid;
					}
				}
				if (enemyIdentifier != null && target.enemyIdentifier == enemyIdentifier && other.TryGetComponent<Rigidbody>(out var component2))
				{
					hasHitTarget = true;
					component2.AddExplosionForce(1000f, base.transform.position, 10f);
					enemyIdentifier.SimpleDamage(damage);
				}
			}
		}
		Flammable component3 = other.GetComponent<Flammable>();
		if ((bool)component3 && !component3.playerOnly)
		{
			component3.Burn(10f);
		}
	}
}
