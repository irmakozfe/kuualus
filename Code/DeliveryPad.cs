using Godot;
using System;
using Thrust.Systems;

namespace Thrust.Levels
{
	public partial class DeliveryPad : LandingPad
	{
		[Export] public int DeliveryPoints = 500;
		[Export] public bool IsLastLevel = true;

		protected override void OnLandingSucceeded()
		{
			    GD.Print($"Delivery landing: rocket={_rocket}, hasCargo={_rocket?.HasCargo}");

			if (_rocket == null || !_rocket.HasCargo)
				return;
		
			_rocket.UnloadCargo();
			GameManager.Instance.AddScore(DeliveryPoints);

			bool allCargoDelivered = AllCargoDelivered();
			GD.Print($"All cargo delivered: {allCargoDelivered}");

			if (allCargoDelivered)
				FinishLevel();
		}

		private bool AllCargoDelivered()
		{
			foreach (Node node in GetTree().GetNodesInGroup(CargoPad.GroupName))
			{
				if (node is CargoPad cargoPad && cargoPad.HasCrate)
					return false;
			}
			return true;
		}

		private void FinishLevel()
		{
			if (IsLastLevel)
				GameManager.Instance.FinishGame();
			else
				GameManager.Instance.NextLevel();
		}
	}
}