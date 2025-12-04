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
    /// 

    public partial class MainWindow : Window
    {
        private Sidakov_DB_PrivateAdsEntities _context = new Sidakov_DB_PrivateAdsEntities();

        public MainWindow()
        {
            InitializeComponent();
            LoadAds();
            LoadFilters();
        }

        public void LoadAds()
        {
            using (var db = new Sidakov_DB_PrivateAdsEntities())
            {
                var ads = db.Ads.ToList();
                AdsList.ItemsSource = ads;
            }
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
            IQueryable<Ads> query = _context.Ads
                .Include(a => a.Cities)
                .Include(a => a.Categories)
                .Include(a => a.Ad_Types)
                .Include(a => a.Ad_Statuses);

            string search = SearchBox.Text.ToLower().Trim();

            if (StatusFilter.SelectedIndex == -1)
            {
                query = query.Where(a => a.Ad_Statuses.status_name == "Активно");
            }


            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.ad_title.ToLower().Contains(search) ||
                    a.ad_description.ToLower().Contains(search)
                );
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

            if (StatusFilter.SelectedItem is Ad_Statuses selectedStatus)
            {
                query = query.Where(a => a.ad_status_id == selectedStatus.status_id);
            }

            AdsList.ItemsSource = query.ToList();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ApplyFiltersAndLoadAds();
        }

    }
}
