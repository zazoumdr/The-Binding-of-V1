using UnityEngine;

namespace ULTRAKILL.Enemy;

public interface ITarget
{
	int Id { get; }

	TargetType Type { get; }

	bool isPlayer => Type == TargetType.PLAYER;

	bool isEnemy => Type == TargetType.ENEMY;

	EnemyIdentifier EID { get; }

	GameObject GameObject { get; }

	Rigidbody Rigidbody { get; }

	Transform Transform { get; }

	Vector3 Position { get; }

	Vector3 HeadPosition { get; }

	void SetData(ref TargetData data);

	void UpdateCachedTransformData();
}
