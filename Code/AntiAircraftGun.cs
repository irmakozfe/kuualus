using Godot;
using System;
using Thrust.Combat;
using Thrust.Configuration;
using Thrust.Effects;
using Thrust.Systems;

namespace Thrust.Characters{
public partial class AntiAircraftGun : StaticBody2D, IDamageable
	{
		[Export] private float _range = 300f;
		[Export] private float _speed = 2.5f;
		[Export] private Node2D _headPivot;
		[Export] private Gun _gun;
		[Export] private float _fireTolerance = 0.1f;
		[Export] private PackedScene _explosionScene;

		private bool _isDestroyed = false;
		private Rocket _rocket;

		private float _aimAngle = -Mathf.Pi / 2;

		public float Range{get {return _range;}}
		public float Speed{get {return _speed;}}
		

		public override void _Ready()
		{

			if(_headPivot == null)
			{
				GD.PushError("Head pivot is not found!");
			}

			_rocket = GetTree().GetFirstNodeInGroup(Config.RocketGroupName) as Rocket;

			if (_rocket == null)
			{
				GD.PushError("Rocket is not found!");
			}
			
		}

		public override void _PhysicsProcess(double delta)
		{
			float deltaTime = (float)delta;
			if (_rocket == null || _rocket.IsDead || _headPivot == null) { return; }

			if (GlobalPosition.DistanceTo(_rocket.GlobalPosition) > _range) { return; }


			Vector2 direction = _rocket.GlobalPosition - _headPivot.GlobalPosition;
			float targetAngle = direction.Angle() - GlobalRotation;


			_aimAngle = Mathf.Wrap(
				Mathf.RotateToward(_aimAngle, targetAngle, Speed * deltaTime),
				-Mathf.Pi, Mathf.Pi);


			_headPivot.Rotation = _aimAngle + Mathf.Pi / 2;

			Shoot(targetAngle);
		}

		private void Shoot(float targetAngle)
		{
			if (_gun == null) {return;}

			bool isAimed = Mathf.Abs(Mathf.AngleDifference(_aimAngle, targetAngle)) < _fireTolerance;

			if (isAimed)
			{
				_gun.Fire(Vector2.FromAngle(_aimAngle + GlobalRotation));
			}
		}

		public void TakeDamage(int amount)
		{
			if (_isDestroyed){ return;}

			_isDestroyed = true;

			if(_explosionScene != null)
			{
				Explosion explosion = _explosionScene.Instantiate<Explosion>();
				explosion.GlobalPosition = GlobalPosition;
				GetTree().CurrentScene.AddChild(explosion);
			}
			//TODO: (maybe) add score to the rocket
			QueueFree();
		}
	}
}
