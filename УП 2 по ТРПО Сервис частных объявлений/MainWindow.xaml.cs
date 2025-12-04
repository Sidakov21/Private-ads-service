using System;
using System.Collections.Generic;
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
using System.Data.Entity;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private Sidakov_DB_PrivateAdsEntities _context = new Sidakov_DB_PrivateAdsEntities();

        public MainWindow()
        {
            InitializeComponent();
            LoadFilters();
            ApplyFiltersAndLoadAds();

        }

        private void LoadFilters()
        {
            // Город
            CityFilter.ItemsSource = _context.Cities.ToList();
            CityFilter.DisplayMemberPath = "city_name";
            // Категория
            CategoryFilter.ItemsSource = _context.Categories.ToList();
            CategoryFilter.DisplayMemberPath = "category_name";
            // Тип
            TypeFilter.ItemsSource = _context.Ad_Types.ToList();
            TypeFilter.DisplayMemberPath = "type_name";
            // Статус
            StatusFilter.ItemsSource = _context.Ad_Statuses.ToList();
            StatusFilter.DisplayMemberPath = "status_name";

            // Добавляем возможность сброса (опционально)
            CityFilter.SelectedIndex = -1;
            CategoryFilter.SelectedIndex = -1;
            TypeFilter.SelectedIndex = -1;
            StatusFilter.SelectedIndex = -1;
        }

        public void ApplyFiltersAndLoadAds()
        {
            using (var db = new Sidakov_DB_PrivateAdsEntities())
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

                if (StatusFilter.SelectedItem is Ad_Statuses selectedStatus)
                {
                    query = query.Where(a => a.ad_status_id == selectedStatus.status_id);
                }
                else
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
    }
}
