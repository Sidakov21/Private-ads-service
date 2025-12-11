using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using УП_2_по_ТРПО_Сервис_частных_объявлений.ВспомогательныеКлассы;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private Sidakov_DB_PrivateAds_TESTEntities _context = new Sidakov_DB_PrivateAds_TESTEntities();

        public MainWindow()
        {
            InitializeComponent();
            LoadFilters();
            UpdateUiForAuthorization();
            ApplyFiltersAndLoadAds();

        }

        private void LoadFilters()
        {
            try
            {

                CityFilter.ItemsSource = _context.Cities.ToList();
                CityFilter.DisplayMemberPath = "city_name";

                CategoryFilter.ItemsSource = _context.Categories.ToList();
                CategoryFilter.DisplayMemberPath = "category_name";

                TypeFilter.ItemsSource = _context.Ad_Types.ToList();
                TypeFilter.DisplayMemberPath = "type_name";

                StatusFilter.ItemsSource = _context.Ad_Statuses.ToList();
                StatusFilter.DisplayMemberPath = "status_name";

                CityFilter.SelectedIndex = -1;
                CategoryFilter.SelectedIndex = -1;
                TypeFilter.SelectedIndex = -1;
                StatusFilter.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров (DB): {ex.Message}", "Ошибка инициализации БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void UpdateUiForAuthorization()
        {
            if (SessionManager.IsAuthorized)
            {
                AddAdButton.Visibility = Visibility.Visible;
                DeleteAdSelectedButton.Visibility = Visibility.Visible;
                ShowMyAdsCheckBox.Visibility = Visibility.Visible;
                ViewCompletedAdsButton.Visibility = Visibility.Visible;

                AuthButton.Content = "Личный кабинет";
            }
            else
            {
                AddAdButton.Visibility = Visibility.Collapsed;
                DeleteAdSelectedButton.Visibility = Visibility.Collapsed;
                ShowMyAdsCheckBox.IsChecked = false;
                ShowMyAdsCheckBox.Visibility = Visibility.Collapsed;
                ViewCompletedAdsButton.Visibility = Visibility.Collapsed;

                AuthButton.Content = "Войти";
            }
        }

        public void ApplyFiltersAndLoadAds()
        {
            using (var db = new Sidakov_DB_PrivateAds_TESTEntities())
            {
                IQueryable<Ads> query = db.Ads
                    .Include(a => a.Cities)
                    .Include(a => a.Categories)
                    .Include(a => a.Ad_Types)
                    .Include(a => a.Ad_Statuses);

                string search = SearchBox.Text.ToLower().Trim();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(a =>
                        a.ad_title.ToLower().Contains(search) ||
                        a.ad_description.ToLower().Contains(search)
                    );
                }

                if (SessionManager.IsAuthorized && ShowMyAdsCheckBox.IsChecked == true)
                {
                    // Фильтруем объявления по ID текущего авторизованного пользователя
                    int currentUserId = SessionManager.CurrentUserId.Value;
                    query = query.Where(a => a.user_id == currentUserId);
                }

                if (StatusFilter.SelectedItem is Ad_Statuses selectedStatus)
                {
                    query = query.Where(a => a.ad_status_id == selectedStatus.status_id);
                }
                else if (ShowMyAdsCheckBox.IsChecked != true)
                {
                    query = query.Where(a => a.Ad_Statuses.status_name == "Активно");
                }

                if (CityFilter.SelectedItem is Cities selectedCity)
                {
                    query = query.Where(a => a.city_id == selectedCity.city_id);
                }

                if (CategoryFilter.SelectedItem is Categories selectedCategory)
                {
                    query = query.Where(a => a.category_id == selectedCategory.category_id);
                }

                if (TypeFilter.SelectedItem is Ad_Types selectedType)
                {
                    query = query.Where(a => a.ad_type_id == selectedType.type_id);
                }

                AdsList.ItemsSource = query.ToList();
            }
        }

        private void AdsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Убеждаемся, что двойной клик произошел на элементе списка, а не на пустом месте
            DependencyObject obj = (DependencyObject)e.OriginalSource;
            while (obj != null && obj != AdsList)
            {
                if (obj is ListViewItem)
                {
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj is ListViewItem && AdsList.SelectedItem is Ads selectedAd)
            {
                if (!SessionManager.IsAuthorized || ShowMyAdsCheckBox.IsChecked != true)
                {
                    MessageBox.Show("Для редактирования необходимо авторизоваться и включить режим 'Мои объявления'.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка, что объявление принадлежит текущему пользователю
                if (selectedAd.user_id != SessionManager.CurrentUserId)
                {
                    MessageBox.Show("Вы можете редактировать только собственные объявления.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AdEditorWindow editor = new AdEditorWindow((int)selectedAd.ad_id);
                editor.ShowDialog();
                ApplyFiltersAndLoadAds();
            }
        }

        private void DeleteAdSelected_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAuthorized || ShowMyAdsCheckBox.IsChecked != true)
            {
                MessageBox.Show("Для удаления объявления необходимо авторизоваться и включить режим 'Мои объявления'.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (AdsList.SelectedItem is Ads selectedAd)
            {
                if (selectedAd.user_id != SessionManager.CurrentUserId)
                {
                    MessageBox.Show("Вы можете удалить только собственные объявления.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    $"Вы действительно хотите удалить объявление \"{selectedAd.ad_title}\"? Это действие нельзя отменить.",
                    "Предупреждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var adToDelete = _context.Ads.Find(selectedAd.ad_id);

                        if (adToDelete != null)
                        {
                            _context.Ads.Remove(adToDelete);
                            _context.SaveChanges();

                            MessageBox.Show("Объявление успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            ApplyFiltersAndLoadAds();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении объявления: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите объявление для удаления из списка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchHint.Visibility = Visibility.Hidden;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchHint.Visibility = Visibility.Visible;
            }
        }

        private void ShowMyAds_Toggle(object sender, RoutedEventArgs e)
        {
            ApplyFiltersAndLoadAds();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ApplyFiltersAndLoadAds();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;

            CityFilter.SelectedIndex = -1;
            CategoryFilter.SelectedIndex = -1;
            TypeFilter.SelectedIndex = -1;
            StatusFilter.SelectedIndex = -1;

            CityFilter.Text = "Выберите город";
            CategoryFilter.Text = "Выберите категорию";
            TypeFilter.Text = "Выберите тип";
            StatusFilter.Text = "Выберите статус";

            ApplyFiltersAndLoadAds();
        }

        private void AddAd_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAuthorized)
            {
                MessageBox.Show("Для добавления объявления необходимо авторизоваться.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AdEditorWindow editor = new AdEditorWindow();
            editor.ShowDialog();

            ApplyFiltersAndLoadAds();
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.IsAuthorized)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите выйти из системы?",
                    "Выход",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SessionManager.Logout();

                    this.Hide();

                    // Открываем окно авторизации заново
                    AuthorizationWindow loginWindow = new AuthorizationWindow();
                    bool? dialogResult = loginWindow.ShowDialog();

                    if (dialogResult == true)
                    {
                        UpdateUiForAuthorization();
                        ApplyFiltersAndLoadAds();
                        this.Show();
                    }
                    else
                    {
                        Application.Current.Shutdown();

                    }
                }
            }
            else
            {
                this.Hide();

                AuthorizationWindow loginWindow = new AuthorizationWindow();
                bool? dialogResult = loginWindow.ShowDialog();

                if (dialogResult == true)
                {
                    UpdateUiForAuthorization();
                    ApplyFiltersAndLoadAds();
                    this.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }
       
        private void ViewCompletedAdsButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedAdsWindow completedAdsWindow = new CompletedAdsWindow();
            completedAdsWindow.ShowDialog();

            ApplyFiltersAndLoadAds();
            UpdateUiForAuthorization();
        }
    }
}
