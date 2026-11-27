using Godot;
using Thrust.Characters;

namespace Thrust.Levels
{
    public partial class LandingPad : StaticBody2D
    {
        [Signal] 
        public delegate void LandingSucceededEventHandler();

        [Export] public float MaxLandingSpeed = 20f; // TODO: See if this does anything
        [Export] public float MaxAngularSpeed = 10f;
        [Export] public float MaxUprightAngle = 15f;
        [Export] public float RequiredStableTime = 2.0f;

        private Area2D _landingZone;
        protected Rocket _rocket;
        private bool _isTouching;
        private bool _landingSuccessful;
        private float _stableTime;

        public override void _Ready()
        {
            _landingZone = GetNode<Area2D>("LandingZone");

            _landingZone.BodyEntered += OnBodyEntered;
            _landingZone.BodyExited += OnBodyExited;
            LandingSucceeded += OnLandingSucceeded;
        
        }

        // To override in the subclasses
        protected virtual void OnLandingSucceeded() { }

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
            
            // GD.Print($"[{Name}] speed={speed:F1}/{MaxLandingSpeed}, " + $"angular={angularSpeed:F1}/{MaxAngularSpeed}, " + $"angle={uprightAngle:F1}/{MaxUprightAngle}, stable={isStable}, time={_stableTime:F2}");

            // Require continuous stability
            if (isStable)
            {
                _stableTime += (float)delta;

                if (_stableTime >= RequiredStableTime)
                {
                    _landingSuccessful = true;

                    GD.Print("LANDING SUCCESSFUL on {Name} ({GetType().Name})!");
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
    }
}