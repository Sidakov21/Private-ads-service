using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений.ВспомогательныеКлассы
{
    public static class SessionManager
    {
        public static int? CurrentUserId { get; private set; }

        // Устанавливает ID пользователя после успешной авторизации.
        public static void SetUser(int userId)
        {
            CurrentUserId = userId;
        }

        // Проверяет, авторизован ли пользователь.
        public static bool IsAuthorized => CurrentUserId.HasValue;

        public static void Logout()
        {
            CurrentUserId = null;
        }
    }
}
