using ULTRAKILL.Cheats;
using UnityEngine;

public class LeviathanTail : MonoBehaviour, IHitTargetCallback
{
	private SwingCheck2[] tails;

	public Vector3[] spawnPositions;

	public Vector3[] secondSpawnPositions;

	private int previousSpawnPosition;

	private Animator anim;

	[HideInInspector]
	public LeviathanController lcon;

	[SerializeField]
	private AudioSource swingSound;

	[SerializeField]
	private AudioSource[] spawnAuds;

	[SerializeField]
	private AudioClip swingHighSound;

	[SerializeField]
	private AudioClip swingLowSound;

	private bool idling;

	private void Awake()
	{
		tails = GetComponentsInChildren<SwingCheck2>();
		anim = GetComponent<Animator>();
		EnemyIdentifier eid = (lcon ? lcon.eid : (eid = lcon.GetComponent<EnemyIdentifier>()));
		SwingCheck2[] array = tails;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].eid = eid;
		}
	}

	private void Update()
	{
		if (idling && !BlindEnemies.Blind)
		{
			idling = false;
			anim.speed = GetAnimSpeed() * lcon.eid.totalSpeedModifier;
			AudioSource[] array = spawnAuds;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play(tracked: true);
			}
		}
	}

	public void TargetBeenHit()
	{
		SwingCheck2[] array = tails;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
	}

	private void SwingStart()
	{
		SwingCheck2[] array = tails;
		foreach (SwingCheck2 obj in array)
		{
			obj.DamageStart();
			obj.col.isTrigger = true;
		}
		Object.Instantiate<AudioSource>(swingSound, base.transform.position, Quaternion.identity).SetPitch(0.5f);
	}

	public void SwingEnd()
	{
		if (tails != null && tails.Length != 0)
		{
			SwingCheck2[] array = tails;
			foreach (SwingCheck2 obj in array)
			{
				obj.DamageStop();
				obj.col.isTrigger = false;
			}
		}
	}

	private void ActionOver()
	{
		base.gameObject.SetActive(value: false);
		lcon.SubAttackOver();
	}

	public void ChangePosition()
	{
		if (lcon.secondPhase)
		{
			Vector3 localPosition = secondSpawnPositions[0];
			float num = Vector3.Distance(base.transform.parent.position + secondSpawnPositions[0], lcon.head.Target.position);
			if (secondSpawnPositions.Length > 1)
			{
				for (int i = 1; i < secondSpawnPositions.Length; i++)
				{
					float num2 = Vector3.Distance(base.transform.parent.position + secondSpawnPositions[i], lcon.head.Target.position);
					if (num2 < num)
					{
						num = num2;
						localPosition = secondSpawnPositions[i];
					}
				}
			}
			base.transform.localPosition = localPosition;
		}
		else
		{
			int num3 = Random.Range(0, spawnPositions.Length);
			if (spawnPositions.Length > 1 && num3 == previousSpawnPosition)
			{
				num3++;
			}
			if (num3 >= spawnPositions.Length)
			{
				num3 = 0;
			}
			if ((bool)lcon.head && lcon.head.gameObject.activeInHierarchy && Vector3.Distance(spawnPositions[num3], new Vector3(lcon.head.transform.localPosition.x, spawnPositions[num3].y, lcon.head.transform.localPosition.z)) < 10f)
			{
				num3++;
			}
			if (num3 >= spawnPositions.Length)
			{
				num3 = 0;
			}
			base.transform.localPosition = spawnPositions[num3];
			previousSpawnPosition = num3;
		}
		bool flag = Random.Range(0f, 1f) > 0.5f;
		base.transform.localPosition += (flag ? (Vector3.up * -30.5f) : (Vector3.up * -4.5f));
		base.transform.localScale = new Vector3((!flag) ? 1 : (-1), 1f, 1f);
		if (lcon.difficulty <= 2)
		{
			spawnAuds[0].clip = (flag ? swingLowSound : swingHighSound);
		}
		if (lcon.secondPhase)
		{
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - new Vector3(lcon.head.transform.position.x, base.transform.position.y, lcon.head.transform.position.z));
		}
		else
		{
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - new Vector3(base.transform.parent.position.x, base.transform.position.y, base.transform.parent.position.z));
		}
		base.gameObject.SetActive(value: true);
		anim.Rebind();
		anim.Update(0f);
		if (BlindEnemies.Blind)
		{
			idling = true;
			anim.speed = 0f;
			return;
		}
		anim.speed = GetAnimSpeed() * lcon.eid.totalSpeedModifier;
		AudioSource[] array = spawnAuds;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Play(tracked: true);
		}
	}

	private void BigSplash()
	{
		Object.Instantiate(lcon.bigSplash, new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z), Quaternion.LookRotation(Vector3.up));
	}

	private float GetAnimSpeed()
	{
		return lcon.difficulty switch
		{
			5 => 2f, 
			4 => 1.5f, 
			3 => 1f, 
			2 => 0.85f, 
			1 => 0.65f, 
			0 => 0.45f, 
			_ => 1f, 
		};
	}

	public void Death()
	{
		SwingEnd();
		if (!(Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		anim.speed = 1f;
		anim.Play("Death", 0, 0f);
	}
}
