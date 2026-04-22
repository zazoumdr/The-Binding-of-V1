using System;
using System.Collections.Generic;
using GameConsole.CommandTree;
using plog;
using plog.Models;

namespace GameConsole.Commands;

internal class Style : CommandRoot, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Style");

	public override string Name => "Style";

	public override string Description => "Modify your style score";

	public Style(Console con)
		: base(con)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("style", BoolMenu("meter", () => MonoSingleton<StyleHUD>.Instance.forceMeterOn, delegate(bool value)
		{
			MonoSingleton<StyleHUD>.Instance.forceMeterOn = value;
		}, inverted: false, requireCheats: true), CommandRoot.Branch("freshness", CommandRoot.Leaf("get", delegate
		{
			Log.Info($"Current weapon freshness is {MonoSingleton<StyleHUD>.Instance.GetFreshness(MonoSingleton<GunControl>.Instance.currentWeapon)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("set", delegate(float amt)
		{
			Log.Info($"Set current weapon freshness to {amt}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<StyleHUD>.Instance.SetFreshness(MonoSingleton<GunControl>.Instance.currentWeapon, amt);
		}, requireCheats: true), CommandRoot.Leaf("lock_state", delegate(int slot, StyleFreshnessState state)
		{
			Log.Info($"Locking slot {slot} to {Enum.GetName(typeof(StyleFreshnessState), state)}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<StyleHUD>.Instance.LockFreshness(slot, state);
		}, requireCheats: true), CommandRoot.Leaf("unlock", delegate(int slot)
		{
			Log.Info($"Unlocking slot {slot}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<StyleHUD>.Instance.UnlockFreshness(slot);
		}, requireCheats: true)));
	}
}
