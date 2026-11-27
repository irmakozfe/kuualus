using System;
using Godot;
using Thrust.Configuration;

namespace Thrust.Effects
{
    [GlobalClass]
    public partial class EffectPlayer: Node2D
    {
        public static EffectPlayer Current {get; private set;}
        
        [Export] private EffectMap _effectMap = null;

        public override void _EnterTree() => Current = this;

        public override void _ExitTree()
        {
            if( Current == this) Current =null;

        }

        public void PlayEffect(EffectType type, Vector2 globalPosition, Action onFinished = null)
        {
            if (type == EffectType.None) return;

            if(_effectMap == null)
            {
                GD.PushError("EffectMap is not assigned.");
                return;
            }

            PackedScene effectScene = _effectMap.GetScene(type);
            if (effectScene == null)
            {
                GD.PushError($"Effect scene matching the effect type {type} not found.");
                return;
            }
            
            Node2D effect = effectScene.Instantiate<Node2D>();
            AddChild(effect);
            effect.GlobalPosition = globalPosition;

            PlayEffect(effect, onFinished);
        }

        private void PlayEffect(Node2D effect, Action onFinished)
        {
            int Playing = 0;
            
            foreach(Node child in effect.GetChildren())
            {
                switch (child)
                {
                    case AnimatedSprite2D sprite:
                    Playing++;
                     sprite.AnimationFinished += () =>
                        {
                            sprite.Hide();
                            EffectFinished();
                        };
                    sprite.Play(Config.ExplodeEffectName);
                    break;

                    case AudioStreamPlayer2D audio:
                    Playing++;
                    audio.Finished += EffectFinished;
                    audio.Play();
                    break;
                }
            }


           if (Playing == 0)
            {
                GD.PushWarning($"Effect {effect.Name} has nothing to play.");
                onFinished?.Invoke();
                effect.QueueFree();
            }

            
            void EffectFinished()
            {
                Playing--;
                if(Playing <= 0)
                {
                    onFinished?.Invoke();
                    effect.QueueFree();
                }
            }

        }


    }
}