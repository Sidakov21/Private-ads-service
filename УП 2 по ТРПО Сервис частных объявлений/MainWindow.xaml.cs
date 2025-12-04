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
        }

        public void LoadAds()
        {
            using (var db = new Sidakov_DB_PrivateAdsEntities())
            {
                var ads = db.Ads.ToList();
                AdsList.ItemsSource = ads;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
