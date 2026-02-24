using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Interfaces
{
    public interface IGameStateService
    {
        GameState CurrentState { get; }
        event EventHandler<GameState> StateChanged;
        void ChangeState(GameState newState);
    }

    /// <summary>
    /// Состояния игры
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

}
