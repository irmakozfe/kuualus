using Godot;

namespace Thrust.Effects
{
    [GlobalClass]
    public partial class EffectMap : Resource
    {
        [Export] private EffectSceneEntry[] _entries = null;

        public PackedScene GetScene(EffectType type)
        {
            foreach(EffectSceneEntry entry in _entries)
            {
                if(entry.EffectType == type)
                {
                    return entry.EffectScene;
                }
            }
            return null;
        }
    }
}