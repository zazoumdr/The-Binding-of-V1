using ScriptableObjects;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class DefaultReferenceManager : MonoSingleton<DefaultReferenceManager>
{
	public GameObject wetParticle;

	public GameObject sandDrip;

	public GameObject blessingGlow;

	public GameObject sandificationEffect;

	public GameObject enrageEffect;

	public GameObject ineffectiveSound;

	public GameObject continuousSplash;

	public GameObject splash;

	public GameObject smallSplash;

	public GameObject bubbles;

	public GameObject projectile;

	public GameObject projectileExplosive;

	public GameObject parryableFlash;

	public GameObject unparryableFlash;

	public GameObject explosion;

	public GameObject superExplosion;

	public Material puppetMaterial;

	public GameObject puppetSpawn;

	public Material blankMaterial;

	public GameObject madnessEffect;

	public LineRenderer electricLine;

	public GameObject zapImpactParticle;

	public FootstepSet footstepSet;

	public GameObject radianceEffect;

	public Shader masterShader;

	public GameObject terminalVelocityEffect;

	public GameObject terminalVelocityExtinguish;

	[Header("Enemies")]
	public GameObject bigJohnator;

	public GameObject cancerousRodent;

	public GameObject centaur;

	public GameObject cerberus;

	public GameObject deathCatcher;

	public GameObject drone;

	public GameObject ferryman;

	public GameObject filth;

	public GameObject fleshPanopticon;

	public GameObject fleshPrison;

	public GameObject gabriel;

	public GameObject gabrielSecond;

	public GameObject gutterman;

	public GameObject guttertank;

	public GameObject hideousMass;

	public GameObject idol;

	public GameObject leviathan;

	public GameObject maliciousFace;

	public GameObject mandalore;

	public GameObject mannequin;

	public GameObject mindflayer;

	public GameObject minosCorpse;

	public GameObject minosPrime;

	public GameObject minotaur;

	public GameObject providence;

	public GameObject puppet;

	public GameObject schism;

	public GameObject sisyphus;

	public GameObject sisyphusPrime;

	public GameObject soldier;

	public GameObject stalker;

	public GameObject stray;

	public GameObject streetcleaner;

	public GameObject swordsmachine;

	public GameObject turret;

	public GameObject v2;

	public GameObject v2second;

	public GameObject veryCancerousRodent;

	public GameObject virtue;

	public GameObject wicked;

	public GameObject GetEnemyPrefab(EnemyType type)
	{
		switch (type)
		{
		case EnemyType.BigJohnator:
			return bigJohnator;
		case EnemyType.CancerousRodent:
			return cancerousRodent;
		case EnemyType.Centaur:
			return centaur;
		case EnemyType.Cerberus:
			return cerberus;
		case EnemyType.Deathcatcher:
			return deathCatcher;
		case EnemyType.Drone:
			return drone;
		case EnemyType.Ferryman:
			return ferryman;
		case EnemyType.Filth:
			return filth;
		case EnemyType.FleshPanopticon:
			return fleshPanopticon;
		case EnemyType.FleshPrison:
			return fleshPrison;
		case EnemyType.Gabriel:
			return gabriel;
		case EnemyType.GabrielSecond:
			return gabrielSecond;
		case EnemyType.Gutterman:
			return gutterman;
		case EnemyType.Guttertank:
			return guttertank;
		case EnemyType.HideousMass:
			return hideousMass;
		case EnemyType.Idol:
			return idol;
		case EnemyType.Leviathan:
			return leviathan;
		case EnemyType.MaliciousFace:
			return maliciousFace;
		case EnemyType.Mandalore:
			return mandalore;
		case EnemyType.Mannequin:
			return mannequin;
		case EnemyType.Mindflayer:
			return mindflayer;
		case EnemyType.Minos:
			return minosCorpse;
		case EnemyType.MinosPrime:
			return minosPrime;
		case EnemyType.Minotaur:
			return minotaur;
		case EnemyType.Providence:
			return providence;
		case EnemyType.Puppet:
			return puppet;
		case EnemyType.Schism:
			return schism;
		case EnemyType.Sisyphus:
			return sisyphus;
		case EnemyType.SisyphusPrime:
			return sisyphusPrime;
		case EnemyType.Soldier:
			return soldier;
		case EnemyType.Stalker:
			return stalker;
		case EnemyType.Stray:
			return stray;
		case EnemyType.Streetcleaner:
			return streetcleaner;
		case EnemyType.Swordsmachine:
			return swordsmachine;
		case EnemyType.Turret:
			return turret;
		case EnemyType.V2:
			return v2;
		case EnemyType.V2Second:
			return v2second;
		case EnemyType.VeryCancerousRodent:
			return veryCancerousRodent;
		case EnemyType.Virtue:
			return virtue;
		case EnemyType.Wicked:
			return wicked;
		default:
			Debug.LogError($"Enemy {type} not found in DefaultReferenceManager");
			return null;
		}
	}
}
