using Godot;
using System;
using System.Collections.Generic;
using Thrust.Systems.State;
using Thrust.Configuration;

namespace Thrust.Systems
{
	public partial class GameManager : Node
	{
		#region Singleton
		public static GameManager Instance { get; private set; }

		public GameManager()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				GD.PushWarning("GameManager instance already exists. Destroying duplicate.");
				QueueFree();
				return;
			}
		}
		#endregion Singleton

		[Signal] public delegate void GameOverEventHandler();
		[Signal] public delegate void ScoreChangedEventHandler(int newScore);
		[Signal] public delegate void RespawnEventHandler();

		private int _score = 0;

		private SceneTree _sceneTree = null;
		private List<GameStateBase> _gameStates = new List<GameStateBase>
		{
			new MainMenuState(),
			new InGameState(),
			new SettingsState(),
			new GameOverState(),
			new WinState(),
			new PauseState(),
		};
		private GameStateBase _nextState = null;

		public Stack<GameStateBase> LoadedStates { get; private set; } = new Stack<GameStateBase>();

		#region Properties
		public GameStateBase ActiveState
		{
			get
			{
				return LoadedStates.Count > 0 ? LoadedStates.Peek() : null;
			}
		}
		public Settings Settings { get; private set; } = null;
		public int Score
		{
			get { return _score; }
			private set
			{
				_score = value;
				EmitSignal(SignalName.ScoreChanged, _score);
			}
		}
		public SceneTree SceneTree
		{
			get 
			{ 
				if (_sceneTree == null)
					_sceneTree = GetTree();

				return _sceneTree;
			}
		}
		#endregion Properties

		public static Vector2I MinWindowSize { get; private set; }

        public override void _Ready()
		{
			// HACK: Initialize the first state
			// TODO: Replace this with a proper implementation
			MinWindowSize = GetWindow().Size;

			// Override currently loaded scene.
			// TODO: Keep Godot's SceneTree up-to-date with currently active scene.
			SceneTree.CurrentScene.QueueFree();

			GameStateBase mainMenu = GetState(GameStateType.MainMenu);
			mainMenu.OnEnter();
			LoadedStates.Push(mainMenu);

			Settings = new Settings();

			// TODO: Don't call this when loading the game from a save file.
			Score = 0;

			// DEBUG: To start the game InGame
			ChangeState(GameStateType.InGame, force: true);
		}

		#region Accessors
		// TODO: Add methods here to modify GameManagers Properties

		/// <summary>
		/// Changes to current game state to a new state
		/// Note: Don't use this to transition to the previous state. For that, use the ActivatePreviousState() method.
		/// </summary>
		/// <param name="targetStateType">The new state to transition to.</param>
		/// <param name="force">Is state activation forced or not.</param>
		/// <returns>True if the state was changed, false otherwise.</returns>
		public bool ChangeState(GameStateType targetStateType, bool force = false)
		{
			GD.Print($"ChangeState {ActiveState?.StateType} -> {targetStateType}, stack: {LoadedStates.Count}");

			// Is the transition from the current state to the target state valid?
			if (ActiveState != null && !ActiveState.CanTransitionTo(targetStateType))
			{
				GD.PushError($"Transition from {ActiveState.StateType} to {targetStateType} is not allowed.");
				return false;
			}
			GameStateBase nextState = GetState(targetStateType);
			if (nextState == null)
			{
				GD.PushError($"State {targetStateType} not found.");
				return false;
			}
			_nextState = nextState;
		
			CallDeferred(nameof(DelayChangeState), force);

			return true;
		}
		/// <summary>
		/// Activates the previous game state.
		/// TODO: Refactor ChangeState to support this corner case and get rid of this method.
		/// </summary>
		/// <returns>True if the previous state was activated, false otherwise.</returns>
		public bool ActivatePreviousState()
		{
			if (LoadedStates.Count < 2)
			{
				GD.PushError("No previous state to activate.");
				return false;
			}

			// Pop the current state from the stack.
			GameStateBase previousState = LoadedStates.Pop();
			previousState.OnExit(keepLoaded: false);

			ActiveState.OnEnter();

			// Activate the previous state, which is now on top of the stack.
			return true;
		}
		public bool StartNewGame()
		{
			if (!ChangeState(GameStateType.InGame, force: true))
			{
				GD.PushError("Failed to start a new game.");
				return false;
			}
			Score = 0;

			// The level scene is added deferred in OnEnter. A deferred call queued
			// after it runs once the scene (and its HUD) is in the tree.
			// Callable.From(ResetGameData).CallDeferred();

			return true;
		}
		public void LostGame()
		{
			if (!ChangeState(GameStateType.GameOver))
			{
				GD.PushError("Failed to change state to GameOver.");
			}
			GD.Print("Game Over!");
			EmitSignal(SignalName.GameOver);
		}
		public void NextLevel()
		{
			if (!ChangeState(GameStateType.InGame, force: true))
			{
				GD.PushError("Failed to change state to InGame.");
			}
			GD.Print("Next Level!");
		}
		public void FinishGame()
		{
			if (!ChangeState(GameStateType.Win))
			{
				GD.PushError("Failed to change state to Win.");
			}
			GD.Print("Game Finished!");
		}
		public void AddScore(int amount)
		{
			if (amount <= 0) return;
			Score += amount;
		}
		#endregion Accessors
		#region Private
		/// <summary>
		///  Finds and returns the state matching with the stateType.
		/// </summary>
		/// <param name="stateType"></param>
		/// <returns></returns>
		private GameStateBase GetState(GameStateType stateType)
		{
			foreach (var state in _gameStates)
			{
				if (state.StateType == stateType)
				{
					return state;
				}
			}
			return null;
		}
		private void DelayChangeState(bool force)
		{
			if (_nextState.IsAdditive)
			{
				ActiveState?.OnExit(keepLoaded: true);
			}
			else
			{
				while (LoadedStates.Count > 0)
				{
					GameStateBase poppedState = LoadedStates.Pop();
					poppedState.OnExit(keepLoaded: false);
				}
			}
			LoadedStates.Push(_nextState);
			_nextState.OnEnter(force);

			_nextState = null;
		}
		#endregion Private
	}
}