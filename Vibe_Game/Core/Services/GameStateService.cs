using System;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Core.Services
{

    public class GameStateService : IGameStateService
    {
        public GameState CurrentState { get; private set; }

        public event EventHandler<GameState> StateChanged;

        public GameStateService(GameState initialState)
        {
            CurrentState = initialState;
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
                return;

            CurrentState = newState;
            StateChanged?.Invoke(this, newState);
        }
    }
}
