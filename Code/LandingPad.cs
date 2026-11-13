using System.Threading.Tasks;
using Godot;
using Thrust.Characters;
using Thrust.Systems;

namespace Thrust.Levels
{
    public partial class LandingPad : StaticBody2D
    {
        [Signal]
        public delegate void LandingSucceededEventHandler();
        [Signal]
        public delegate void CargoLoadedEventHandler();

        [Export] public float MaxLandingSpeed = 20f; // TODO: See if this does anything
        [Export] public float MaxAngularSpeed = 10f;
        [Export] public float MaxUprightAngle = 15f;
        [Export] public float RequiredStableTime = 2.0f;
        
        // CARGO
        [Export] public bool IsDeliveryPad = false;
        [Export] public Sprite2D Crate = null;
        [Export] public float CargoMass = 2;
        [Export] public int DeliveryPoints = 500;
        [Export] private Line2D _beam = null;

        private Area2D _landingZone;
        private Rocket _rocket;
        private bool _isTouching;
        private bool _landingSuccessful;
        private float _stableTime;

        // CARGO
        private bool _isLoading = false;

        public override void _Ready()
        {
            _landingZone = GetNode<Area2D>("LandingZone");

            _landingZone.BodyEntered += OnBodyEntered;
            _landingZone.BodyExited += OnBodyExited;
            LandingSucceeded += OnLandingSuccessful;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_rocket == null)
                return;

            // Check physical contact with this landing pad
            bool touching = false;

            foreach (Node2D body in _rocket.GetCollidingBodies())
            {
                if (body == this)
                {
                    touching = true;
                    break;
                }
            }

            // Reset landing when the rocket leaves the surface
            if (!touching)
            {
                if (_isTouching)
                    GD.Print("Rocket no longer touching landing pad");

                _isTouching = false;
                _landingSuccessful = false;
                _stableTime = 0f;
                return;
            }

            if (!_isTouching)
            {
                _isTouching = true;
                GD.Print("Rocket touching landing pad");
            }

            // Prevent repeated success while still landed
            if (_landingSuccessful)
                return;

            // Check current movement speed
            float speed = _rocket.LinearVelocity.Length();

            // Check current angular speed
            float angularSpeed = Mathf.Abs(
                Mathf.RadToDeg(_rocket.AngularVelocity)
            );

            // Check upright orientation
            float uprightAngle = Mathf.Abs(
                Mathf.RadToDeg(
                    Mathf.Wrap(
                        _rocket.GlobalRotation,
                        -Mathf.Pi,
                        Mathf.Pi
                    )
                )
            );

            bool isStable =
                speed <= MaxLandingSpeed &&
                angularSpeed <= MaxAngularSpeed &&
                uprightAngle <= MaxUprightAngle;

            // Require continuous stability
            if (isStable)
            {
                _stableTime += (float)delta;

                if (_stableTime >= RequiredStableTime)
                {
                    _landingSuccessful = true;

                    GD.Print("LANDING SUCCESSFUL!");
                    EmitSignal(SignalName.LandingSucceeded);
                }
            }
            else
            {
                _stableTime = 0f;
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is Rocket rocket)
            {
                _rocket = rocket;
                _isTouching = false;
                _landingSuccessful = false;
                _stableTime = 0f;

                GD.Print("Rocket entered landing zone");
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body == _rocket)
            {
                _rocket = null;
                _isTouching = false;
                _landingSuccessful = false;
                _stableTime = 0f;

                GD.Print("Rocket left landing zone");
            }
        }
        #region Private
        private async void OnLandingSuccessful()
        {
            if (_isLoading) return;

            if (Crate != null && !_rocket.HasCargo)
            {
                await LoadCargo();
            }
            else if (IsDeliveryPad && _rocket.HasCargo)
            {
                _rocket.UnloadCargo();
                // TODO: Add points through GameManager
                GameManager.Instance.AddScore(DeliveryPoints);
            }
        }

        private async Task LoadCargo()
        {
            _isLoading = true;

            _rocket.LinearVelocity = Vector2.Zero;
            _rocket.AngularVelocity = 0f;
            _rocket.SetDeferred(RigidBody2D.PropertyName.Freeze, true);

            _beam.Points = new[]
            {
                _beam.ToLocal(_rocket.GlobalPosition),
                _beam.ToLocal(Crate.GlobalPosition)
            };

            var tween = CreateTween();
            tween.TweenProperty(_beam, "width", 16, .2);
            tween.TweenProperty(Crate, "global_position", _rocket.GlobalPosition, 1.0)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);
            tween.Parallel().TweenProperty(Crate, "scale", Crate.Scale * .7f, 1.0);
            tween.TweenProperty(_beam, "width", 0, .2);

            await ToSignal(tween, Tween.SignalName.Finished);

            Crate.QueueFree();
            Crate = null;

            _rocket.LoadCargo(CargoMass);
            _rocket.SetDeferred(RigidBody2D.PropertyName.Freeze, false);

            _isLoading = false;
            EmitSignal(SignalName.CargoLoaded);
        }
        #endregion Private
    }
}
