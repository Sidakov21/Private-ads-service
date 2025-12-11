using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using УП_2_по_ТРПО_Сервис_частных_объявлений.ВспомогательныеКлассы; // Должен содержать SessionManager

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    public partial class CompletedAdsWindow : Window
    {
        private Sidakov_DB_PrivateAdsEntities1 _context = new Sidakov_DB_PrivateAdsEntities1();

        public CompletedAdsWindow()
        {
            InitializeComponent();
            LoadCompletedAdsAndProfit();
        }

        private void LoadCompletedAdsAndProfit()
        {
            if (!SessionManager.IsAuthorized)
            {
                MessageBox.Show("Для просмотра завершенных объявлений необходимо авторизоваться.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            try
            {
                int currentUserId = SessionManager.CurrentUserId.Value;

                var completedStatus = _context.Ad_Statuses.FirstOrDefault(s => s.status_name == "Завершено");
                if (completedStatus == null)
                {
                    MessageBox.Show("Статус 'Завершено' не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Загружаем завершенные объявления текущего пользователя
                var completedAds = _context.Ads
                    .Where(a => a.user_id == currentUserId && a.ad_status_id == completedStatus.status_id)
                    .OrderByDescending(a => a.ad_post_date) // Сортировка по дате
                    .ToList();

                // Отображаем список
                CompletedAdsList.ItemsSource = completedAds;

                //Отображаем общую прибыль
                var currentUser = _context.Users.Find(currentUserId);
                if (currentUser != null)
                {
                    decimal totalProfit = currentUser.profit ?? 0;
                    TotalProfitTextBlock.Text = $"{totalProfit:N2} руб."; // Форматируем для красивого вывода
                }
                else
                {
                    TotalProfitTextBlock.Text = "Пользователь не найден.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}