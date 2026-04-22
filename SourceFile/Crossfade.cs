using System.Collections.Generic;
using UnityEngine;

public class Crossfade : MonoBehaviour
{
	public bool multipleTargets;

	public AudioSource from;

	public AudioSource to;

	public AudioSource[] froms;

	public AudioSource[] tos;

	[HideInInspector]
	public float[] fromMaxVolumes;

	[HideInInspector]
	public float[] toOriginalVolumes;

	[HideInInspector]
	public float[] toMaxVolumes;

	[HideInInspector]
	public float[] toMinVolumes;

	[HideInInspector]
	public bool inProgress;

	public float time;

	public bool unscaledTime;

	private float fadeAmount;

	public bool match;

	public bool dontActivateOnStart;

	public bool oneTime;

	private bool activated;

	private bool firstTime = true;

	private static HashSet<AudioSource> _hadPlayOnAwake = new HashSet<AudioSource>();

	private void Awake()
	{
		if (!multipleTargets)
		{
			if ((bool)(Object)(object)from)
			{
				froms = (AudioSource[])(object)new AudioSource[1];
				froms[0] = from;
			}
			else
			{
				froms = (AudioSource[])(object)new AudioSource[0];
			}
			if ((bool)(Object)(object)to)
			{
				tos = (AudioSource[])(object)new AudioSource[1];
				tos[0] = to;
			}
			else
			{
				tos = (AudioSource[])(object)new AudioSource[0];
			}
		}
		if (fromMaxVolumes == null || fromMaxVolumes.Length == 0)
		{
			fromMaxVolumes = new float[froms.Length];
		}
		if (toOriginalVolumes == null || toOriginalVolumes.Length == 0)
		{
			toOriginalVolumes = new float[tos.Length];
		}
		if (toMaxVolumes == null || toMaxVolumes.Length == 0)
		{
			toMaxVolumes = new float[tos.Length];
		}
		if (toMinVolumes == null || toMinVolumes.Length == 0)
		{
			toMinVolumes = new float[tos.Length];
		}
		if (tos.Length != 0)
		{
			for (int i = 0; i < tos.Length; i++)
			{
				toOriginalVolumes[i] = tos[i].volume;
			}
		}
		for (int j = 0; j < froms.Length; j++)
		{
			if ((Object)(object)froms[j] != null && froms[j].playOnAwake)
			{
				_hadPlayOnAwake.Add(froms[j]);
				froms[j].playOnAwake = false;
			}
		}
		for (int k = 0; k < tos.Length; k++)
		{
			if ((Object)(object)tos[k] != null && tos[k].playOnAwake)
			{
				_hadPlayOnAwake.Add(tos[k]);
				tos[k].playOnAwake = false;
			}
		}
	}

	private void Start()
	{
		for (int i = 0; i < froms.Length; i++)
		{
			if ((Object)(object)froms[i] != null && _hadPlayOnAwake.Contains(froms[i]) && !froms[i].isPlaying)
			{
				froms[i].Play();
			}
		}
		if (!dontActivateOnStart && !inProgress)
		{
			StartFade();
		}
	}

	private void OnEnable()
	{
		if (!dontActivateOnStart && !inProgress)
		{
			StartFade();
		}
	}

	private void Update()
	{
		if (!inProgress)
		{
			return;
		}
		fadeAmount = Mathf.MoveTowards(fadeAmount, 1f, (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / time);
		if (froms.Length != 0)
		{
			for (int i = 0; i < froms.Length; i++)
			{
				if (!((Object)(object)froms[i] == null))
				{
					froms[i].volume = Mathf.Lerp(fromMaxVolumes[i], 0f, fadeAmount);
				}
			}
		}
		if (tos.Length != 0)
		{
			for (int j = 0; j < tos.Length; j++)
			{
				if (!((Object)(object)tos[j] == null))
				{
					tos[j].volume = Mathf.Lerp(toMinVolumes[j], toMaxVolumes[j], fadeAmount);
				}
			}
		}
		if (fadeAmount == 1f)
		{
			StopFade();
		}
	}

	public void StartFade()
	{
		if (!activated)
		{
			activated = true;
		}
		else if (oneTime)
		{
			return;
		}
		if (froms.Length != 0)
		{
			for (int i = 0; i < froms.Length; i++)
			{
				if ((Object)(object)froms[i] == null)
				{
					continue;
				}
				if (MonoSingleton<CrossfadeTracker>.Instance.actives.Count > 0)
				{
					for (int num = MonoSingleton<CrossfadeTracker>.Instance.actives.Count - 1; num >= 0; num--)
					{
						if (!(MonoSingleton<CrossfadeTracker>.Instance.actives[num] == null))
						{
							if (MonoSingleton<CrossfadeTracker>.Instance.actives[num].froms != null && MonoSingleton<CrossfadeTracker>.Instance.actives[num].froms.Length != 0)
							{
								for (int num2 = MonoSingleton<CrossfadeTracker>.Instance.actives[num].froms.Length - 1; num2 >= 0; num2--)
								{
									if (!((Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num].froms[num2] == null) && (Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num].froms[num2] == (Object)(object)froms[i])
									{
										MonoSingleton<CrossfadeTracker>.Instance.actives[num].StopFade();
									}
								}
							}
							if (MonoSingleton<CrossfadeTracker>.Instance.actives[num].tos != null && MonoSingleton<CrossfadeTracker>.Instance.actives[num].tos.Length != 0)
							{
								for (int num3 = MonoSingleton<CrossfadeTracker>.Instance.actives[num].tos.Length - 1; num3 >= 0; num3--)
								{
									if (!((Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num].tos[num3] == null) && (Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num].tos[num3] == (Object)(object)froms[i])
									{
										MonoSingleton<CrossfadeTracker>.Instance.actives[num].StopFade();
									}
								}
							}
						}
					}
				}
				if (fromMaxVolumes != null && fromMaxVolumes.Length != 0)
				{
					fromMaxVolumes[i] = froms[i].volume;
				}
			}
		}
		if (tos.Length != 0)
		{
			for (int j = 0; j < tos.Length; j++)
			{
				if ((Object)(object)tos[j] == null)
				{
					continue;
				}
				if (MonoSingleton<CrossfadeTracker>.Instance.actives.Count > 0)
				{
					bool flag = false;
					for (int num4 = MonoSingleton<CrossfadeTracker>.Instance.actives.Count - 1; num4 >= 0; num4--)
					{
						if (!(MonoSingleton<CrossfadeTracker>.Instance.actives[num4] == null))
						{
							if (MonoSingleton<CrossfadeTracker>.Instance.actives[num4].froms != null && MonoSingleton<CrossfadeTracker>.Instance.actives[num4].froms.Length != 0)
							{
								for (int num5 = MonoSingleton<CrossfadeTracker>.Instance.actives[num4].froms.Length - 1; num5 >= 0; num5--)
								{
									if (!((Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num4].froms[num5] == null) && (Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num4].froms[num5] == (Object)(object)tos[j])
									{
										flag = true;
									}
								}
							}
							if (MonoSingleton<CrossfadeTracker>.Instance.actives[num4].tos != null && MonoSingleton<CrossfadeTracker>.Instance.actives[num4].tos.Length != 0)
							{
								for (int num6 = MonoSingleton<CrossfadeTracker>.Instance.actives[num4].tos.Length - 1; num6 >= 0; num6--)
								{
									if (!((Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num4].tos[num6] == null) && (Object)(object)MonoSingleton<CrossfadeTracker>.Instance.actives[num4].tos[num6] == (Object)(object)tos[j])
									{
										flag = true;
									}
								}
							}
							if (flag)
							{
								MonoSingleton<CrossfadeTracker>.Instance.actives[num4].StopFade();
								toMinVolumes[j] = tos[j].volume;
							}
						}
					}
					if (!flag && firstTime)
					{
						tos[j].volume = 0f;
					}
				}
				else if (firstTime)
				{
					tos[j].volume = 0f;
				}
				if (toMinVolumes != null && toMinVolumes.Length != 0)
				{
					toMinVolumes[j] = tos[j].volume;
				}
				if (toMaxVolumes != null && toMaxVolumes.Length != 0)
				{
					toMaxVolumes[j] = toOriginalVolumes[j];
				}
				if (match && froms.Length != 0)
				{
					tos[j].Play(tracked: true);
					tos[j].Pause();
					tos[j].time = froms[0].time % tos[j].clip.length;
					tos[j].UnPause();
				}
				else if (!tos[j].isPlaying)
				{
					tos[j].Play(tracked: true);
				}
			}
		}
		MonoSingleton<CrossfadeTracker>.Instance.actives.Add(this);
		fadeAmount = 0f;
		inProgress = true;
		firstTime = false;
	}

	public void StopFade()
	{
		if (inProgress)
		{
			inProgress = false;
			if (MonoSingleton<CrossfadeTracker>.Instance.actives.Contains(this))
			{
				MonoSingleton<CrossfadeTracker>.Instance.actives.Remove(this);
			}
		}
	}
}
