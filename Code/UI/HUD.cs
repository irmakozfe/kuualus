using Godot;
using System;
using Thrust.Characters;
using Thrust.UI;

namespace Thrust.IU{
public partial class HUD : CanvasLayer
	{
		[Export] private Rocket _rocket;
		[Export] private HealthBar _healthbar;
		[Export] private FuelBar _fuelbar;

		public override void _Ready()
		{
			_healthbar?.Bind(_rocket);
			_fuelbar?.Bind(_rocket);
		}
	}
}
