using System.Linq;
using ULTRAKILL.Cheats;
using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
	public bool oneTime;

	public bool disableOnExit;

	public bool dontActivateOnEnable;

	public bool reactivateOnEnable;

	public bool activateOnDisable;

	public bool forEnemies;

	public bool notIfEnemiesDisabled;

	public bool onlyIfPlayerIsAlive;

	public bool dontUseEventsIfEnemiesDisabled;

	public bool onAwake;

	[HideInInspector]
	public bool activated;

	[HideInInspector]
	public bool activating;

	public float delay;

	private bool nonCollider;

	private int playerIn;

	[Space(20f)]
	public Collider[] ignoreColliders;

	[Space(20f)]
	public ObjectActivationCheck obac;

	public bool onlyCheckObacOnce;

	public bool disableIfObacOff;

	[Space(10f)]
	public UltrakillEvent events;

	private bool canUseEvents
	{
		get
		{
			if (DisableEnemySpawns.DisableArenaTriggers)
			{
				return !dontUseEventsIfEnemiesDisabled;
			}
			return true;
		}
	}

	private GameObject playerObject
	{
		get
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.FPS)
			{
				return MonoSingleton<PlatformerMovement>.Instance.GameObject;
			}
			return MonoSingleton<NewMovement>.Instance.GameObject;
		}
	}

	private bool playerDead
	{
		get
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.FPS)
			{
				return MonoSingleton<PlatformerMovement>.Instance.dead;
			}
			return MonoSingleton<NewMovement>.Instance.dead;
		}
	}

	private void Awake()
	{
		if (onAwake)
		{
			StartCheck();
		}
	}

	private void Start()
	{
		if (!onAwake)
		{
			StartCheck();
		}
	}

	private void StartCheck()
	{
		if (dontActivateOnEnable || !(GetComponent<Collider>() == null) || !(GetComponent<Rigidbody>() == null))
		{
			return;
		}
		nonCollider = true;
		if ((!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !playerDead) && (!oneTime || (!activating && !activated)))
		{
			if (delay == 0f)
			{
				Activate();
			}
			else
			{
				Invoke("Activate", delay);
			}
		}
	}

	private void Update()
	{
		if ((nonCollider || playerIn > 0) && !activating && !activated && (bool)obac && obac.readyToActivate && !onlyCheckObacOnce && (!onlyIfPlayerIsAlive || !playerDead))
		{
			activating = true;
			Invoke("Activate", delay);
		}
		if (disableIfObacOff && activated && (bool)obac && !obac.readyToActivate)
		{
			Deactivate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ignoreColliders != null && ignoreColliders.Contains(other))
		{
			return;
		}
		if ((forEnemies && other.gameObject.CompareTag("Enemy")) || (!forEnemies && other.gameObject == playerObject))
		{
			playerIn++;
		}
		if (((!forEnemies && (!oneTime || (!activating && !activated)) && other.gameObject == playerObject) || (forEnemies && !activating && !activated && other.gameObject.CompareTag("Enemy"))) && playerIn == 1 && (!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !playerDead))
		{
			if (oneTime)
			{
				activating = true;
			}
			Invoke("Activate", delay);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (ignoreColliders == null || !ignoreColliders.Contains(other))
		{
			if ((forEnemies && other.gameObject.CompareTag("Enemy")) || (!forEnemies && other.gameObject == playerObject))
			{
				playerIn--;
			}
			if (disableOnExit && ((!forEnemies && (activating || activated) && other.gameObject == playerObject && playerIn == 0) || (forEnemies && (activating || activated) && other.gameObject.CompareTag("Enemy"))) && (!onlyIfPlayerIsAlive || !playerDead))
			{
				Deactivate();
			}
		}
	}

	public void ActivateDelayed(float delay)
	{
		Invoke("Activate", delay);
	}

	public void Activate()
	{
		Activate(false);
	}

	public void Activate(bool ignoreDisabled = false)
	{
		if ((base.gameObject.activeSelf || ignoreDisabled) && (!activated || !oneTime) && (!onlyIfPlayerIsAlive || !playerDead) && (!notIfEnemiesDisabled || !DisableEnemySpawns.DisableArenaTriggers) && (!obac || obac.readyToActivate))
		{
			activating = false;
			activated = true;
			if (canUseEvents)
			{
				events.Invoke();
			}
		}
	}

	public void Deactivate()
	{
		if (!oneTime)
		{
			activated = false;
			activating = false;
		}
		if (canUseEvents)
		{
			events.Revert();
		}
		CancelInvoke("Activate");
	}

	public void Switch()
	{
		if (activated)
		{
			Deactivate();
		}
		else
		{
			Activate();
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded)
		{
			activating = false;
			playerIn = 0;
			CancelInvoke("Activate");
			if ((!activated || !oneTime) && activateOnDisable && (!notIfEnemiesDisabled || !DisableEnemySpawns.DisableArenaTriggers) && (!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !playerDead))
			{
				Activate(ignoreDisabled: true);
			}
			else if (activated && nonCollider && disableOnExit && (!onlyIfPlayerIsAlive || !playerDead))
			{
				Deactivate();
			}
		}
	}

	private void OnEnable()
	{
		if ((!activated || reactivateOnEnable) && nonCollider && (!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !playerDead))
		{
			activating = true;
			Invoke("Activate", delay);
		}
	}
}
