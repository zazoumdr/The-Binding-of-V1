using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects;

[CreateAssetMenu(fileName = "Footstep Set", menuName = "ULTRAKILL/FootstepSet")]
public class FootstepSet : ScriptableObject
{
	[Serializable]
	public class Footsteps
	{
		[field: SerializeField]
		public SurfaceType SurfaceType { get; private set; }

		[field: SerializeField]
		public AudioClip[] Clips { get; private set; }
	}

	[Serializable]
	public class EnviroGibs
	{
		[field: SerializeField]
		public SurfaceType SurfaceType { get; private set; }

		[field: SerializeField]
		public GameObject[] gibs { get; private set; }
	}

	[Serializable]
	public class EnviroGibParticles
	{
		[field: SerializeField]
		public SurfaceType SurfaceType { get; private set; }

		[field: SerializeField]
		public GameObject particle { get; private set; }
	}

	[Serializable]
	public class SlideParticles
	{
		[field: SerializeField]
		public SurfaceType SurfaceType { get; private set; }

		[field: SerializeField]
		public GameObject particle { get; private set; }
	}

	[Serializable]
	public class WallScrapeParticles
	{
		[field: SerializeField]
		public SurfaceType SurfaceType { get; private set; }

		[field: SerializeField]
		public GameObject particle { get; private set; }
	}

	[SerializeField]
	private Footsteps[] footsteps;

	[SerializeField]
	private EnviroGibs[] enviroGibs;

	[SerializeField]
	private EnviroGibParticles[] enviroGibParticles;

	[SerializeField]
	private SlideParticles[] slideParticles;

	[SerializeField]
	private WallScrapeParticles[] wallScrapeParticles;

	[NonSerialized]
	private Dictionary<SurfaceType, AudioClip[]> footstepsDictionary;

	[NonSerialized]
	private Dictionary<SurfaceType, GameObject[]> enviroGibsDictionary;

	[NonSerialized]
	private Dictionary<SurfaceType, GameObject> enviroGibParticleDictionary;

	[NonSerialized]
	private Dictionary<SurfaceType, GameObject> slideParticlesDictionary;

	[NonSerialized]
	private Dictionary<SurfaceType, GameObject> wallScrapeParticlesDictionary;

	[NonSerialized]
	private bool initialized;

	public bool TryGetFootstepClips(SurfaceType surfaceType, out AudioClip[] clips)
	{
		Initialize();
		if (footstepsDictionary.TryGetValue(surfaceType, out clips) || footstepsDictionary.TryGetValue(SurfaceType.Generic, out clips))
		{
			return true;
		}
		return false;
	}

	public bool TryGetEnviroGibs(SurfaceType surfaceType, out GameObject[] enviroGibs)
	{
		Initialize();
		if (enviroGibsDictionary.TryGetValue(surfaceType, out enviroGibs) || enviroGibsDictionary.TryGetValue(SurfaceType.Generic, out enviroGibs))
		{
			return true;
		}
		return false;
	}

	public bool TryGetEnviroGibParticle(SurfaceType surface, out GameObject particle)
	{
		Initialize();
		if (enviroGibParticleDictionary.TryGetValue(surface, out particle) || enviroGibParticleDictionary.TryGetValue(SurfaceType.Generic, out particle))
		{
			return true;
		}
		return false;
	}

	public bool TryGetSlideParticle(SurfaceType surface, out GameObject particle)
	{
		Initialize();
		if (slideParticlesDictionary.TryGetValue(surface, out particle) || slideParticlesDictionary.TryGetValue(SurfaceType.Generic, out particle))
		{
			return true;
		}
		return false;
	}

	public bool TryGetWallScrapeParticle(SurfaceType surface, out GameObject particle)
	{
		Initialize();
		if (wallScrapeParticlesDictionary.TryGetValue(surface, out particle) || wallScrapeParticlesDictionary.TryGetValue(SurfaceType.Generic, out particle))
		{
			return true;
		}
		return false;
	}

	private void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			footstepsDictionary = new Dictionary<SurfaceType, AudioClip[]>();
			Footsteps[] array = this.footsteps;
			foreach (Footsteps footsteps in array)
			{
				footstepsDictionary[footsteps.SurfaceType] = footsteps.Clips;
			}
			enviroGibsDictionary = new Dictionary<SurfaceType, GameObject[]>();
			EnviroGibs[] array2 = this.enviroGibs;
			foreach (EnviroGibs enviroGibs in array2)
			{
				enviroGibsDictionary[enviroGibs.SurfaceType] = enviroGibs.gibs;
			}
			enviroGibParticleDictionary = new Dictionary<SurfaceType, GameObject>();
			EnviroGibParticles[] array3 = this.enviroGibParticles;
			foreach (EnviroGibParticles enviroGibParticles in array3)
			{
				enviroGibParticleDictionary[enviroGibParticles.SurfaceType] = enviroGibParticles.particle;
			}
			slideParticlesDictionary = new Dictionary<SurfaceType, GameObject>();
			SlideParticles[] array4 = this.slideParticles;
			foreach (SlideParticles slideParticles in array4)
			{
				slideParticlesDictionary[slideParticles.SurfaceType] = slideParticles.particle;
			}
			wallScrapeParticlesDictionary = new Dictionary<SurfaceType, GameObject>();
			WallScrapeParticles[] array5 = this.wallScrapeParticles;
			foreach (WallScrapeParticles wallScrapeParticles in array5)
			{
				wallScrapeParticlesDictionary[wallScrapeParticles.SurfaceType] = wallScrapeParticles.particle;
			}
		}
	}
}
