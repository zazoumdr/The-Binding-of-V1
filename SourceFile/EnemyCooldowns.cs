using System.Collections.Generic;
using UnityEngine;

public class EnemyCooldowns : MonoSingleton<EnemyCooldowns>
{
	public float virtueCooldown;

	public float ferrymanCooldown;

	public Power attackingPower;

	public int previousPowerMove;

	public List<Enemy> currentVirtues = new List<Enemy>();

	public List<Ferryman> ferrymen = new List<Ferryman>();

	public List<Power> powers = new List<Power>();

	private void Start()
	{
		Debug.Log("Enemy Cooldowns", this);
		SlowUpdate();
	}

	private void Update()
	{
		if (virtueCooldown > 0f)
		{
			virtueCooldown = Mathf.MoveTowards(virtueCooldown, 0f, Time.deltaTime);
		}
		if (ferrymanCooldown > 0f)
		{
			ferrymanCooldown = Mathf.MoveTowards(ferrymanCooldown, 0f, Time.deltaTime);
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 10f);
		for (int num = currentVirtues.Count - 1; num >= 0; num--)
		{
			if (currentVirtues[num] == null || !currentVirtues[num].gameObject.activeInHierarchy)
			{
				currentVirtues.RemoveAt(num);
			}
		}
		for (int num2 = ferrymen.Count - 1; num2 >= 0; num2--)
		{
			if (ferrymen[num2] == null || !ferrymen[num2].gameObject.activeInHierarchy)
			{
				ferrymen.RemoveAt(num2);
			}
		}
	}

	public void PowerAttacking(Power pwr)
	{
		if (pwr != null)
		{
			attackingPower = pwr;
		}
	}

	public void PowerAttackEnd()
	{
		powers.Remove(attackingPower);
		powers.Add(attackingPower);
		attackingPower = null;
		RefreshPowers();
	}

	public void RefreshPowers()
	{
		int num = -1;
		if (powers == null || powers.Count == 0)
		{
			return;
		}
		for (int num2 = powers.Count - 1; num2 >= 0; num2--)
		{
			if (powers[num2] == null)
			{
				powers.RemoveAt(num2);
			}
			if (powers[num2].enraged)
			{
				num = num2;
			}
		}
		if (num > -1)
		{
			Power item = powers[num];
			powers.RemoveAt(num);
			powers.Insert(0, item);
		}
	}

	public void PrioritizePower(Power pwr)
	{
		ChangePowerPriority(0, pwr);
	}

	public void DeprioritizePower(Power pwr)
	{
		ChangePowerPriority(powers.Count - 1, pwr);
	}

	private void ChangePowerPriority(int index, Power pwr)
	{
		if (powers != null && powers.Count != 0 && powers.Contains(pwr) && powers.IndexOf(pwr) != index)
		{
			powers.Remove(pwr);
			powers.Insert(index, pwr);
		}
	}

	public void AddVirtue(Enemy vrt)
	{
		if (currentVirtues.Count <= 0 || !currentVirtues.Contains(vrt))
		{
			currentVirtues.Add(vrt);
		}
	}

	public void RemoveVirtue(Enemy vrt)
	{
		if (currentVirtues.Count > 0 && currentVirtues.Contains(vrt))
		{
			currentVirtues.Remove(vrt);
		}
	}

	public void AddFerryman(Ferryman fm)
	{
		if (ferrymen.Count <= 0 || !ferrymen.Contains(fm))
		{
			ferrymen.Add(fm);
		}
	}

	public void RemoveFerryman(Ferryman fm)
	{
		if (ferrymen.Count > 0 && ferrymen.Contains(fm))
		{
			ferrymen.Remove(fm);
		}
	}

	public void AddPower(Power pwr)
	{
		if (powers.Count <= 0 || !powers.Contains(pwr))
		{
			powers.Add(pwr);
		}
	}

	public void RemovePower(Power pwr)
	{
		if (powers.Count > 0 && powers.Contains(pwr))
		{
			powers.Remove(pwr);
		}
	}
}
