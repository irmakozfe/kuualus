using Godot;
using System;
using Thrust.Effects;

namespace Thrust.Combat{
public partial class Gun : Node2D
{
	[Export] private PackedScene _bulletScene = null;
	[Export] private float _cooldown = 1.2f;

	[Export] private float _bulletSpeed = 0f;

	[Export] private EffectType _fireEffect = EffectType.None;
	
	private float _cooldownLeft;
	public bool CanFire
	{
		get
		{
			return _bulletScene != null && _cooldownLeft <= 0f;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float deltaTime = (float) delta;

		if(_cooldownLeft > 0f)
		{
			_cooldownLeft -= deltaTime;
		}
	}


	public bool Fire(Vector2 direction)
	{
		if (!CanFire)
		{
			return false;
		}


		Bullet bullet = _bulletScene.Instantiate<Bullet>();

		if (_bulletSpeed > 0f)
		{
			bullet.Speed = _bulletSpeed;   
		}

		GetTree().CurrentScene.AddChild(bullet);
		bullet.Launch(GlobalPosition, direction);

		EffectPlayer.Current?.PlayEffect(_fireEffect, GlobalPosition);
		_cooldownLeft = _cooldown;
		
		return true;
	}
}
}
