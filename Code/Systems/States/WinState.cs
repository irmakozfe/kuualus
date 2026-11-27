namespace Thrust.Systems.State
{
    public class WinState : GameStateBase
    {
        public override GameStateType StateType => GameStateType.Win;
        public override string ScenePath => "res://Scenes/Menus/Win.tscn";

        public WinState()
        {
            AddTargetState(GameStateType.InGame);
            AddTargetState(GameStateType.MainMenu);
        }
    }
}