using Godot;
using Thrust.Configuration;

namespace Thrust.Effects
{
	public partial class Explosion : AnimatedSprite2D
	{
		[Export] private AudioStreamPlayer2D _explosionSound;
		[Signal] public delegate void FinishedEventHandler();


		public override void _Ready()
		{
			AnimationFinished += OnAnimationFinished;
			Play(Config.ExplodeEffectName);
			_explosionSound?.Play();
		}

		private void OnAnimationFinished()
		{
			Hide();

			if(_explosionSound != null && _explosionSound.Playing)
			{
				_explosionSound.Finished += Finish; 
			}
			else
			{
				Finish();
			}
		}


		private void Finish()
		{
			EmitSignal(SignalName.Finished);
			QueueFree();
		}
	}
}
