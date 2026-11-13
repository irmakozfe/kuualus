using Godot;
using System;
using Thrust.Characters;

namespace Thrust.UI
{
	public partial class FuelBar : TextureProgressBar
		{
		private Rocket _rocket;

		public void Bind(Rocket rocket)
		{
			if(rocket == null)
			{
				GD.PushError("No rocket found to assign the health bar");
				return;
			}

			_rocket = rocket;

			MaxValue = _rocket.MaxFuel;
			Value = _rocket.MaxFuel;

			_rocket.FuelChanged += OnFuelChanged;
		}

		public override void _ExitTree()
		{
			if(_rocket != null)
			{
				_rocket.HealthChanged -= OnFuelChanged;
			}
		}

		private void OnFuelChanged(int current, int max)
		{
			MaxValue = max;
			Value = current;
		}
	}
}