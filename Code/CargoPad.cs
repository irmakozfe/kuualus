using System.Threading.Tasks;
using Godot;
using Thrust.Characters;
using Thrust.Systems;

namespace Thrust.Levels
{
    public partial class CargoPad : LandingPad
    {
        public const string GroupName = "CargoPads";
        [Signal]
        public delegate void CargoLoadedEventHandler();

        [Export] public Sprite2D Crate = null;
        [Export] public float CargoMass = 2;
        [Export] private Line2D _beam = null;

        private bool _isLoading = false;
        public bool HasCrate => Crate != null;

        public override void _Ready()
        {
            base._Ready();
            AddToGroup(GroupName);
        }

        #region Private
        protected override async void OnLandingSucceeded()
        {
            GD.Print($"CargoPad landing: loading={_isLoading}, crate={Crate}, rocket={_rocket}, hasCargo={_rocket?.HasCargo}");
            if (_isLoading || Crate == null ||
                _rocket == null || _rocket.HasCargo) 
                    return;

            await LoadCargo(_rocket);
        }

        private async Task LoadCargo(Rocket rocket)
        {
            _isLoading = true;

            rocket.LinearVelocity = Vector2.Zero;
            rocket.AngularVelocity = 0f;
            rocket.SetDeferred(RigidBody2D.PropertyName.Freeze, true);

            _beam.Points = new[]
            {
                _beam.ToLocal(rocket.GlobalPosition),
                _beam.ToLocal(Crate.GlobalPosition)
            };

            var tween = CreateTween();
            tween.TweenProperty(_beam, "width", 16, .2);
            tween.TweenProperty(Crate, "global_position", rocket.GlobalPosition, 1.0)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);
            tween.Parallel().TweenProperty(Crate, "scale", Crate.Scale * .7f, 1.0);
            tween.TweenProperty(_beam, "width", 0, .2);

            await ToSignal(tween, Tween.SignalName.Finished);

            Crate.QueueFree();
            Crate = null;

            rocket.LoadCargo(CargoMass);
            rocket.SetDeferred(RigidBody2D.PropertyName.Freeze, false);

            _isLoading = false;
            EmitSignal(SignalName.CargoLoaded);
        }
        #endregion Private
    }
}
