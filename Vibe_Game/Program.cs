// IsaacClone.Game/Program.cs
using System;

namespace Vibe_Game
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
            {
                try
                {
                    game.Run();
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    LogError(ex);

#if DEBUG
                    // В дебаге показываем окно с ошибкой
                    System.Windows.Forms.MessageBox.Show(
                        $"Ошибка запуска игры:\n{ex.Message}\n\n{ex.StackTrace}",
                        "Isaac Clone - Ошибка",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
#endif
                }
            }
        }

        static void LogError(Exception ex)
        {
            // Запись ошибки в файл
            string logPath = "error.log";
            string logMessage = $"[{DateTime.Now}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";

            try
            {
                System.IO.File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Не удалось записать лог - игнорируем
            }
        }
    }
}