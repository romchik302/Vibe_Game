using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibe_Game.Core.IExample;

namespace Vibe_Game.Core.Interfaces
{

    /// <summary>
    /// Репозиторий для сохранения статистики
    /// </summary>
    public interface IStatsRepository
    {
        void SaveRunStats(RunStats stats);
        RunStats LoadBestStats();
        List<RunStats> LoadAllStats();
    }

    /// <summary>
    /// Статистика пробега
    /// </summary>
    public class RunStats
    {
        public int Score { get; set; }
        public float PlayTime { get; set; }
        public int Floor { get; set; }
        public DateTime Date { get; set; }
    }
}
