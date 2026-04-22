using System;
using System.Collections.Generic;

public static class EnemyTypes
{
	public static readonly HashSet<Type> Types = new HashSet<Type>
	{
		typeof(Enemy),
		typeof(ZombieMelee),
		typeof(Stalker),
		typeof(StatueBoss),
		typeof(Mass),
		typeof(DroneFlesh),
		typeof(V2),
		typeof(SpiderBody),
		typeof(Gutterman),
		typeof(Guttertank),
		typeof(Sisyphus),
		typeof(MortarLauncher),
		typeof(Deathcatcher),
		typeof(Mandalore),
		typeof(Mindflayer),
		typeof(Turret),
		typeof(MaliciousFace),
		typeof(Idol),
		typeof(GabrielBase),
		typeof(Gabriel),
		typeof(GabrielSecond),
		typeof(Power),
		typeof(MinosPrime),
		typeof(SisyphusPrime)
	};

	public static string GetEnemyName(EnemyType type)
	{
		return type switch
		{
			EnemyType.Gabriel => "Gabriel, Judge of Hell", 
			EnemyType.GabrielSecond => "Gabriel, Apostate of Hate", 
			EnemyType.Mandalore => "Mysterious Druid Knight (& Owl)", 
			EnemyType.Sisyphus => "Sisyphean Insurrectionist", 
			EnemyType.Turret => "Sentry", 
			EnemyType.V2Second => "V2", 
			_ => Enum.GetName(typeof(EnemyType), type), 
		};
	}
}
