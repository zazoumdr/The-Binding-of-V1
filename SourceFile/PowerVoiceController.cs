using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PowerVoiceController : MonoSingleton<PowerVoiceController>
{
	[SerializeField]
	private AudioClip[] intro;

	private int lastIntro = -1;

	public TimeSince sinceLastIntro;

	[SerializeField]
	private AudioClip[] enrage;

	private int lastEnrage = -1;

	[SerializeField]
	private AudioClip[] taunt;

	private int lastTaunt = -1;

	[SerializeField]
	private AudioClip[] cheapShot;

	private int lastCheapShot = -1;

	[SerializeField]
	private AudioClip[] hurt;

	private int lastHurt = -1;

	[SerializeField]
	private AudioClip[] hurtBig;

	private int lastHurtBig = -1;

	[SerializeField]
	private AudioClip[] death;

	private int lastDeath = -1;

	[SerializeField]
	private AudioClip[] rapier;

	private int lastRapier = -1;

	[SerializeField]
	private AudioClip[] greatsword;

	private int lastGreatsword = -1;

	[SerializeField]
	private AudioClip[] spear;

	private int lastSpear = -1;

	[SerializeField]
	private AudioClip[] spearThrow;

	private int lastSpearThrow = -1;

	[SerializeField]
	private AudioClip[] glaive;

	private int lastGlaive = -1;

	[SerializeField]
	private AudioClip[] glaiveThrow;

	private int lastGlaiveThrow = -1;

	[SerializeField]
	private AudioClip fallScream;

	private void Awake()
	{
		sinceLastIntro = 3f;
	}

	public AudioClip GetSound(AudioClip[] clips, int lastNumber, out int newNumber)
	{
		if (clips.Length <= 1)
		{
			newNumber = -1;
			if (clips.Length != 1)
			{
				return null;
			}
			return clips[0];
		}
		int num = Random.Range(0, clips.Length);
		if (num == lastNumber)
		{
			num = ((num != clips.Length - 1) ? (num + 1) : 0);
		}
		newNumber = num;
		return clips[num];
	}

	public AudioClip Intro()
	{
		if ((float)sinceLastIntro < 3f)
		{
			return null;
		}
		AudioClip sound = GetSound(intro, lastIntro, out lastIntro);
		string caption = "";
		switch (lastIntro)
		{
		case 0:
			caption = "Be afraid, machine.";
			break;
		case 1:
			caption = "Here shall be your grave.";
			break;
		case 2:
			caption = "It is over, machine!";
			break;
		case 3:
			caption = "Surrender or perish!";
			break;
		case 4:
			caption = "Lay down and die!";
			break;
		}
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(caption);
		return sound;
	}

	public AudioClip Enrage()
	{
		AudioClip sound = GetSound(enrage, lastEnrage, out lastEnrage);
		string caption = "";
		switch (lastEnrage)
		{
		case 0:
			caption = "Bastard!";
			break;
		case 1:
			caption = "You piece of SHIT!";
			break;
		case 2:
			caption = "Just DIE already!";
			break;
		case 3:
			caption = "Why won't you die!?";
			break;
		case 4:
			caption = "God DAMN it!";
			break;
		}
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(caption);
		return sound;
	}

	public AudioClip Taunt()
	{
		AudioClip sound = GetSound(taunt, lastTaunt, out lastTaunt);
		string caption = "";
		switch (lastTaunt)
		{
		case 0:
			caption = "This lowly thing could never have bested him!";
			break;
		case 1:
			caption = "An inconvenience at best.";
			break;
		case 2:
			caption = "This is a waste of my time!";
			break;
		case 3:
			caption = "Just another worthless object.";
			break;
		}
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(caption);
		return sound;
	}

	public AudioClip CheapShot()
	{
		AudioClip sound = GetSound(cheapShot, lastCheapShot, out lastCheapShot);
		string caption = "";
		switch (lastCheapShot)
		{
		case 0:
			caption = "PAY ATTENTION!";
			break;
		case 1:
			caption = "Wait your TURN!";
			break;
		case 2:
			caption = "WRONG TARGET!";
			break;
		}
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(caption);
		return sound;
	}

	public AudioClip Hurt()
	{
		return GetSound(hurt, lastHurt, out lastHurt);
	}

	public AudioClip HurtBig()
	{
		return GetSound(hurtBig, lastHurtBig, out lastHurtBig);
	}

	public AudioClip Death()
	{
		return GetSound(death, lastDeath, out lastDeath);
	}

	public AudioClip Rapier()
	{
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Rapier!");
		return GetSound(rapier, lastRapier, out lastRapier);
	}

	public AudioClip Greatsword()
	{
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Greatsword!");
		return GetSound(greatsword, lastGreatsword, out lastGreatsword);
	}

	public AudioClip Spear()
	{
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Spear!");
		return GetSound(spear, lastSpear, out lastSpear);
	}

	public AudioClip SpearThrow()
	{
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Over here!");
		return GetSound(spearThrow, lastSpearThrow, out lastSpearThrow);
	}

	public AudioClip Glaive()
	{
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Glaive!");
		return GetSound(glaive, lastGlaive, out lastGlaive);
	}

	public AudioClip GlaiveThrow()
	{
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Take THIS!");
		return GetSound(glaiveThrow, lastGlaiveThrow, out lastGlaiveThrow);
	}

	public AudioClip FallScream()
	{
		return fallScream;
	}
}
