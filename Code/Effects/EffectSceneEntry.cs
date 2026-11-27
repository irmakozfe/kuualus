using Godot;

namespace Thrust.Effects
{
    [GlobalClass]
    public partial class EffectSceneEntry : Resource
    {
        [Export] public EffectType EffectType {get; set;}
        [Export] public PackedScene EffectScene {get; set;}
    }
    
}