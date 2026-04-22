using UnityEngine;

public static class LayerMaskDefaults
{
	public static bool IsMatchingLayer(int otherLayer, LMD layerMask)
	{
		return ((int)Get(layerMask) & (1 << otherLayer)) != 0;
	}

	public static LayerMask Get(LMD lmd)
	{
		LayerMask layerMask = default(LayerMask);
		switch (lmd)
		{
		case LMD.Enemies:
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.Environment:
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x40;
			return (int)layerMask | 0x80;
		case LMD.Player:
			return (int)layerMask | 4;
		case LMD.EnvironmentAndBigEnemies:
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x40;
			layerMask = (int)layerMask | 0x80;
			return (int)layerMask | 0x800;
		case LMD.EnemiesAndEnvironment:
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x40;
			layerMask = (int)layerMask | 0x80;
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.EnemiesAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.EnvironmentAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x40;
			return (int)layerMask | 0x80;
		case LMD.EnemiesEnvironmentAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x40;
			layerMask = (int)layerMask | 0x80;
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.BigEnemiesEnvironmentAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x40;
			layerMask = (int)layerMask | 0x80;
			return (int)layerMask | 0x800;
		default:
			return layerMask;
		}
	}
}
