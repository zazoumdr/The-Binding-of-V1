using TMPro;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class StyleCalculator : MonoSingleton<StyleCalculator>
{
	public StyleHUD shud;

	private GameObject player;

	private NewMovement nmov;

	public TMP_Text airTimeText;

	public float airTime = 1f;

	private Vector3 airTimePos;

	private StatsManager sman;

	private GunControl gc;

	public bool enemiesShot;

	public float multikillTimer;

	public int multikillCount;

	private string maxAirTime = "<color=red><size=72>x3.00</size></color>";

	private float lastAirTime;

	private void Start()
	{
		shud = MonoSingleton<StyleHUD>.Instance;
		nmov = MonoSingleton<NewMovement>.Instance;
		player = nmov.gameObject;
		airTimePos = airTimeText.transform.localPosition;
		sman = MonoSingleton<StatsManager>.Instance;
		gc = nmov.GetComponentInChildren<GunControl>();
	}

	private void Update()
	{
		if (!nmov.gc.onGround || nmov.sliding)
		{
			airTime = Mathf.MoveTowards(airTime, 3f, Time.deltaTime * 2f);
			if (!((Component)(object)airTimeText).gameObject.activeSelf)
			{
				((Component)(object)airTimeText).gameObject.SetActive(value: true);
			}
		}
		else if (!nmov.boost)
		{
			airTime = Mathf.MoveTowards(airTime, 1f, Time.deltaTime * 10f);
			airTimeText.transform.localPosition = airTimePos;
		}
		if (airTime >= 2f && airTime < 3f)
		{
			if (lastAirTime != airTime)
			{
				airTimeText.text = "<color=orange><size=60>x" + airTime.ToString("F2") + "</size></color>";
			}
			airTimeText.transform.localPosition = new Vector3(airTimePos.x + (float)Random.Range(-3, 3), airTimePos.y + (float)Random.Range(-3, 3), airTimePos.z);
		}
		else if (airTime == 3f)
		{
			if (lastAirTime != airTime)
			{
				airTimeText.text = maxAirTime;
			}
			airTimeText.transform.localPosition = new Vector3(airTimePos.x + (float)Random.Range(-6, 6), airTimePos.y + (float)Random.Range(-6, 6), airTimePos.z);
		}
		else if (airTime == 1f && ((Component)(object)airTimeText).gameObject.activeSelf)
		{
			((Component)(object)airTimeText).gameObject.SetActive(value: false);
		}
		else
		{
			if (lastAirTime != airTime)
			{
				airTimeText.text = "x" + airTime.ToString("F2");
			}
			airTimeText.transform.localPosition = airTimePos;
		}
		if (multikillTimer > 0f)
		{
			multikillTimer -= Time.deltaTime * 10f;
		}
		else if (multikillCount != 0)
		{
			multikillTimer = 0f;
			multikillCount = 0;
		}
		lastAirTime = airTime;
	}

	public void HitCalculator(string hitter, string enemyType, string hitLimb, bool dead, EnemyIdentifier eid = null, GameObject sourceWeapon = null)
	{
		if ((eid != null && eid.blessed) || MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			return;
		}
		switch (hitter)
		{
		case "punch":
		case "heavypunch":
			if (dead)
			{
				if (hitLimb == "head" || hitLimb == "limb")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(60, "ultrakill.criticalpunch", eid, sourceWeapon2);
				}
				else if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(150, "ultrakill.bigfistkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(30, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else if (enemyType == "spider")
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(60, "ultrakill.disrespect", eid, sourceWeapon2);
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(20, "", eid, sourceWeapon2);
			}
			break;
		case "ground slam":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(60, "ultrakill.groundslam", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(20, "", eid, sourceWeapon2);
			}
			break;
		case "revolver":
			enemiesShot = true;
			if (dead)
			{
				if (hitLimb == "head" && enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(150, "ultrakill.bigheadshot", eid, sourceWeapon2);
				}
				else if (hitLimb == "head")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(80, "ultrakill.headshot", eid, sourceWeapon2);
				}
				else if (hitLimb == "limb")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(60, "ultrakill.limbhit", eid, sourceWeapon2);
				}
				else if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(100, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(30, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else if (hitLimb == "head")
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(25, "", eid, sourceWeapon2);
			}
			else if (hitLimb == "limb")
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(15, "", eid, sourceWeapon2);
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(10, "", eid, sourceWeapon2);
			}
			break;
		case "shotgun":
			enemiesShot = true;
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(100, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(45, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(4, "ultrakill.shotgunhit", eid, sourceWeapon2);
			}
			break;
		case "hammer":
			if (dead)
			{
				gc.AddKill();
			}
			break;
		case "shotgunzone":
			enemiesShot = true;
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(125, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(100, "ultrakill.overkill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			break;
		case "nail":
		case "sawblade":
			enemiesShot = true;
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(100, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(45, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(2, "ultrakill.nailhit", eid, sourceWeapon2);
			}
			break;
		case "railcannon":
			enemiesShot = true;
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(100, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(45, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(15, "", eid, sourceWeapon2);
			}
			break;
		case "zapper":
			enemiesShot = true;
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(175, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(80, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(60, "ultrakill.zapperhit", eid, sourceWeapon2);
			}
			break;
		case "lightningbolt":
			enemiesShot = true;
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(300, "ultrakill.lightningbolt", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(250, "ultrakill.lightningbolt", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(200, "ultrakill.lightningbolt", eid, sourceWeapon2);
			}
			break;
		case "projectile":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(250, "ultrakill.friendlyfire", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(200, "ultrakill.friendlyfire", eid, sourceWeapon2);
			}
			break;
		case "ffexplosion":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(250, "ultrakill.friendlyfire", eid, sourceWeapon2);
				sourceWeapon2 = sourceWeapon;
				AddPoints(0, "ultrakill.exploded", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(200, "ultrakill.friendlyfire", eid, sourceWeapon2);
			}
			break;
		case "explosion":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(45, "ultrakill.exploded", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(15, "ultrakill.explosionhit", eid, sourceWeapon2);
			}
			break;
		case "fire":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(20, "FRIED", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(2, "ultrakill.firehit", eid, sourceWeapon2);
			}
			break;
		case "harpoon":
			if (dead)
			{
				if (enemyType == "spider")
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(100, "ultrakill.bigkill", eid, sourceWeapon2);
				}
				else
				{
					GameObject sourceWeapon2 = sourceWeapon;
					AddPoints(45, "ultrakill.kill", eid, sourceWeapon2);
				}
				gc.AddKill();
			}
			break;
		case "chainsawprojectile":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(80, "UNCHAINEDSAW", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(8, "ultrakill.nailhit", eid, sourceWeapon2);
			}
			break;
		case "chainsaw":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(60, "NO-NO", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(4, "ultrakill.nailhit", eid, sourceWeapon2);
			}
			break;
		case "chainsawbounce":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(100, "RE-NO-NO", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(6, "ultrakill.nailhit", eid, sourceWeapon2);
			}
			break;
		case "chainsawzone":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(60, "GROOVY", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(4, "ultrakill.drillhit", eid, sourceWeapon2);
			}
			break;
		case "drill":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(120, "SCREWED", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(4, "ultrakill.drillhit", eid, sourceWeapon2);
			}
			break;
		case "drillpunch":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(120, "ultrakill.drillpunchkill", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(60, "ultrakill.drillpunch", eid, sourceWeapon2);
			}
			break;
		case "cannonball":
			if (dead)
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(75, "ultrakill.cannonballed", eid, sourceWeapon2);
				gc.AddKill();
			}
			else
			{
				GameObject sourceWeapon2 = sourceWeapon;
				AddPoints(25, "", eid, sourceWeapon2);
			}
			break;
		}
		if (dead && !eid.puppet && hitter != "secret")
		{
			AddToMultiKill(sourceWeapon);
		}
	}

	public void AddToMultiKill(GameObject sourceWeapon = null)
	{
		if (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer)
		{
			multikillCount++;
			multikillTimer = 5f;
			switch (multikillCount)
			{
			case 2:
				shud.AddPoints(25, "ultrakill.doublekill", sourceWeapon);
				return;
			case 3:
				shud.AddPoints(50, "ultrakill.triplekill", sourceWeapon);
				return;
			case 0:
			case 1:
				return;
			}
			StyleHUD styleHUD = shud;
			int count = multikillCount;
			styleHUD.AddPoints(100, "ultrakill.multikill", sourceWeapon, null, count);
		}
	}

	private void AddPoints(int points, string pointName, EnemyIdentifier eid, GameObject sourceWeapon = null)
	{
		int num = Mathf.RoundToInt((float)points * airTime - (float)points);
		shud.AddPoints(points + num, pointName, sourceWeapon, eid);
	}
}
