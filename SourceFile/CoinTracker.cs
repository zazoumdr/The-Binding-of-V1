using System.Collections.Generic;
using System.Threading;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class CoinTracker : MonoSingleton<CoinTracker>
{
	public readonly List<Coin> revolverCoinsList = new List<Coin>();

	private CancellationTokenSource untrackTargetsSource;

	private CancellationToken untrackTargetsToken;

	private CancellationTokenSource untrackCoinsSource;

	private CancellationToken untrackCoinsToken;

	public readonly Dictionary<ITarget, CancellationToken> targets = new Dictionary<ITarget, CancellationToken>();

	private void Start()
	{
		Invoke("SlowUpdate", 30f);
	}

	public void Reset()
	{
		ResetUntrackTargetsToken();
		ResetUntrackCoinsToken();
		if (revolverCoinsList.Count <= 0)
		{
			return;
		}
		for (int num = revolverCoinsList.Count - 1; num >= 0; num--)
		{
			if (!revolverCoinsList[num].dontDestroyOnPlayerRespawn)
			{
				Object.Destroy(revolverCoinsList[num].gameObject);
				revolverCoinsList.RemoveAt(num);
			}
		}
	}

	private void ResetUntrackTargetsToken()
	{
		if (untrackTargetsSource != null)
		{
			untrackTargetsSource.Cancel();
			untrackTargetsSource.Dispose();
			untrackTargetsSource = null;
		}
	}

	private void ResetUntrackCoinsToken()
	{
		if (untrackCoinsSource != null)
		{
			untrackCoinsSource.Cancel();
			untrackCoinsSource.Dispose();
			untrackCoinsSource = null;
		}
	}

	public void AddCoin(Coin coin)
	{
		if (revolverCoinsList.Contains(coin))
		{
			return;
		}
		revolverCoinsList.Add(coin);
		coin.destroyCancellationToken.Register(delegate
		{
			RemoveCoin(coin);
		});
		PortalManagerV2 portalManagerV = MonoSingleton<PortalManagerV2>.Instance;
		if (!portalManagerV)
		{
			return;
		}
		TargetTracker targetTracker = portalManagerV.TargetTracker;
		if (revolverCoinsList.Count == 1)
		{
			untrackTargetsSource = new CancellationTokenSource();
			untrackTargetsToken = untrackTargetsSource.Token;
			{
				foreach (KeyValuePair<ITarget, CancellationToken> target in targets)
				{
					RegisterTargetToTracker(targetTracker, target.Key, target.Value);
				}
				return;
			}
		}
		if (revolverCoinsList.Count == 2)
		{
			untrackCoinsSource = new CancellationTokenSource();
			untrackCoinsToken = untrackCoinsSource.Token;
			for (int num = 0; num < revolverCoinsList.Count; num++)
			{
				RegisterCoinToTracker(targetTracker, revolverCoinsList[num]);
			}
		}
		else if (revolverCoinsList.Count > 2)
		{
			RegisterCoinToTracker(targetTracker, coin);
		}
	}

	private void RemoveCoin(Coin coin)
	{
		revolverCoinsList.Remove(coin);
		if (revolverCoinsList.Count == 0)
		{
			ResetUntrackTargetsToken();
		}
		if (revolverCoinsList.Count == 1)
		{
			ResetUntrackCoinsToken();
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 30f);
		for (int num = revolverCoinsList.Count - 1; num >= 0; num--)
		{
			if (revolverCoinsList[num] == null)
			{
				revolverCoinsList.RemoveAt(num);
			}
		}
	}

	public void RegisterTarget(ITarget target, CancellationToken token)
	{
		targets.Add(target, token);
		if (revolverCoinsList.Count > 0)
		{
			PortalManagerV2 portalManagerV = MonoSingleton<PortalManagerV2>.Instance;
			if ((bool)portalManagerV)
			{
				RegisterTargetToTracker(portalManagerV.TargetTracker, target, token);
			}
		}
		token.Register(delegate
		{
			targets.Remove(target);
		});
	}

	private void RegisterCoinToTracker(TargetTracker tt, Coin coin)
	{
		CancellationToken token = CreateLinkedToken(untrackCoinsToken, coin.destroyCancellationToken);
		tt.RegisterTarget(coin, token);
	}

	private void RegisterTargetToTracker(TargetTracker tt, ITarget target, CancellationToken token)
	{
		CancellationToken token2 = CreateLinkedToken(untrackTargetsToken, token);
		tt.RegisterTarget(target, token2);
	}

	private static CancellationToken CreateLinkedToken(CancellationToken t1, CancellationToken t2)
	{
		return CancellationTokenSource.CreateLinkedTokenSource(t1, t2).Token;
	}
}
