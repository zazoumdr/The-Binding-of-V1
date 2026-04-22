using System.Collections.Generic;
using GameConsole.CommandTree;
using plog;
using plog.Models;
using UnityEngine;

namespace GameConsole.Commands;

internal class Buffs : CommandRoot, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Buffs");

	public override string Name => "Buffs";

	public override string Description => "Modify buffs for enemies";

	public Buffs(Console con)
		: base(con)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("buffs", BoolMenu("forceradiance", () => OptionsManager.forceRadiance, delegate(bool value)
		{
			OptionsManager.forceRadiance = value;
			EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateBuffs();
			}
		}, inverted: false, requireCheats: true), BoolMenu("forcesand", () => OptionsManager.forceSand, delegate(bool value)
		{
			OptionsManager.forceSand = value;
			EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Sandify();
			}
		}, inverted: false, requireCheats: true), BoolMenu("forcepuppet", () => OptionsManager.forcePuppet, delegate(bool value)
		{
			OptionsManager.forcePuppet = value;
			EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].PuppetSpawn();
			}
		}, inverted: false, requireCheats: true), BoolMenu("forcebossbars", () => OptionsManager.forceBossBars, delegate(bool value)
		{
			OptionsManager.forceBossBars = value;
			EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].BossBar(value);
			}
		}, inverted: false, requireCheats: true), CommandRoot.Branch("radiancetier", CommandRoot.Leaf("get", delegate
		{
			Log.Info($"Current radiance tier is {OptionsManager.radianceTier}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("set", delegate(float amt)
		{
			Log.Info($"Set current radiance tier to {amt}", (IEnumerable<Tag>)null, (string)null, (object)null);
			OptionsManager.radianceTier = amt;
			EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateBuffs();
			}
		}, requireCheats: true)));
	}
}
