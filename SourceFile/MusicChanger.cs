using UnityEngine;

public class MusicChanger : MonoBehaviour
{
	public bool match;

	public bool oneTime;

	public bool onEnable;

	public bool dontStart;

	public bool startBattleVersion;

	public bool forceOn;

	public float pitch = 1f;

	public AudioClip clean;

	public AudioClip battle;

	public AudioClip boss;

	private MusicManager muman;

	private void OnEnable()
	{
		if (onEnable)
		{
			Change();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!onEnable && other.gameObject.CompareTag("Player"))
		{
			Change();
		}
	}

	public void ChangeTo(AudioClip clip)
	{
		clean = clip;
		battle = clip;
		boss = clip;
		Change();
	}

	public void Change()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (muman == null)
		{
			muman = MonoSingleton<MusicManager>.Instance;
		}
		if (!oneTime && !((Object)(object)muman.cleanTheme.clip != (Object)(object)clean) && !((Object)(object)muman.battleTheme.clip != (Object)(object)battle) && (!muman.off || (muman.forcedOff && !forceOn)))
		{
			return;
		}
		float time = 0f;
		bool off = muman.off;
		if (match)
		{
			time = (((Object)(object)muman.cleanTheme.clip == null) ? muman.battleTheme.time : muman.cleanTheme.time);
		}
		else
		{
			muman.cleanTheme.time = 0f;
			muman.battleTheme.time = 0f;
			muman.bossTheme.time = 0f;
		}
		muman.StopMusic();
		muman.cleanTheme.SetPitch(pitch);
		muman.battleTheme.SetPitch(pitch);
		muman.bossTheme.SetPitch(pitch);
		muman.cleanTheme.clip = clean;
		muman.battleTheme.clip = battle;
		muman.bossTheme.clip = boss;
		if (forceOn)
		{
			muman.forcedOff = false;
		}
		if (!dontStart || !off)
		{
			if (startBattleVersion)
			{
				muman.ArenaMusicStart(goIntoArenaMode: false);
			}
			else
			{
				muman.StartMusic();
			}
		}
		if (match)
		{
			muman.cleanTheme.time = time;
			muman.battleTheme.time = time;
			muman.bossTheme.time = time;
		}
		if (oneTime)
		{
			Object.Destroy(this);
		}
	}

	public void ChangePitch(float newPitch)
	{
		pitch = newPitch;
	}
}
