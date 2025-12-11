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
using System.Windows.Shapes;
using УП_2_по_ТРПО_Сервис_частных_объявлений.ВспомогательныеКлассы;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    /// <summary>
    /// Логика взаимодействия для AdEditorWindow.xaml
    /// </summary>
    public partial class AdEditorWindow : Window
    {
        private readonly int? _adId; 
        private Sidakov_DB_PrivateAds_TESTEntities _context = new Sidakov_DB_PrivateAds_TESTEntities();

        private Ad_Statuses _originalAdStatus;
        private bool _isAdAlreadyCompleted = false;

        private string _selectedPhotoPath;

        // Конструктор для ДОБАВЛЕНИЯ нового объявления
        public AdEditorWindow()
        {
            InitializeComponent();
            _adId = null;
            InitializeForm();
        }

        // Конструктор для РЕДАКТИРОВАНИЯ существующего объявления
        public AdEditorWindow(int adId)
        {
            InitializeComponent();
            _adId = adId;
            InitializeForm();
        }

        private void InitializeForm()
        {
            LoadComboBoxes();

            if (_adId.HasValue)
            {
                Title = "Редактирование объявления";
                TitleBlock.Text = "Редактирование объявления";
                LoadAdData();
            }
            else
            {
                Title = "Добавление объявления";
                TitleBlock.Text = "Добавление нового объявления";
            }
        }

        private void LoadComboBoxes()
        {
            CityBox.ItemsSource = _context.Cities.ToList();
            CityBox.DisplayMemberPath = "city_name";
            CityBox.SelectedValuePath = "city_id";

            CategoryBox.ItemsSource = _context.Categories.ToList();
            CategoryBox.DisplayMemberPath = "category_name";
            CategoryBox.SelectedValuePath = "category_id";

            TypeBox.ItemsSource = _context.Ad_Types.ToList();
            TypeBox.DisplayMemberPath = "type_name";
            TypeBox.SelectedValuePath = "type_id";

            StatusBox.ItemsSource = _context.Ad_Statuses.ToList();
            StatusBox.DisplayMemberPath = "status_name";
            StatusBox.SelectedValuePath = "status_id";

            if (!_adId.HasValue)
            {
                var activeStatus = _context.Ad_Statuses.FirstOrDefault(s => s.status_name == "Активно");
                if (activeStatus != null)
                {
                    StatusBox.SelectedItem = activeStatus;
                }
            }
        }

        private void LoadAdData()
        {
            if (!_adId.HasValue) return;

            var adToEdit = _context.Ads
                .Include(a => a.Cities)
                .Include(a => a.Categories)
                .Include(a => a.Ad_Types)
                .Include(a => a.Ad_Statuses)
                .FirstOrDefault(a => a.ad_id == _adId.Value);

            if (adToEdit == null)
            {
                MessageBox.Show("Объявление не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }


            _originalAdStatus = adToEdit.Ad_Statuses;
            _isAdAlreadyCompleted = adToEdit.Ad_Statuses.status_name == "Завершено";


            TitleBox.Text = adToEdit.ad_title;
            DescriptionBox.Text = adToEdit.ad_description;
            PriceBox.Text = adToEdit.price.ToString();

            CityBox.SelectedItem = adToEdit.Cities;
            CategoryBox.SelectedItem = adToEdit.Categories;
            TypeBox.SelectedItem = adToEdit.Ad_Types;
            StatusBox.SelectedItem = adToEdit.Ad_Statuses;

            //Загрузка сущесвтующего фото
            if (!string.IsNullOrEmpty(adToEdit.ad_photo_path))
            {
                try
                {
                    // Предполагаем, что ad_photo_path хранится относительно папки приложения (например, "Images/файл.jpg")
                    string photoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, adToEdit.ad_photo_path.TrimStart('/', '\\'));
                    AdImagePreview.Source = new BitmapImage(new Uri(photoPath));
                    PhotoTextBlock.Text = $"Фотография: {System.IO.Path.GetFileName(photoPath)}"; // Показываем имя файла
                    _selectedPhotoPath = adToEdit.ad_photo_path; // Сохраняем существующий путь
                }
                catch
                {
                    // Игнорируем ошибку загрузки, если файл не найден или путь некорректен
                    AdImagePreview.Source = null;
                    PhotoTextBlock.Text = "Фото не найдено.";
                }
            }

            // Блокировка полей, если объявление уже завершено
            if (_isAdAlreadyCompleted)
            {
                StatusBox.IsEnabled = false;
                ProfitBox.IsEnabled = false;
            }

            StatusBox_SelectionChanged(StatusBox, null);
        }

        private void StatusBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusBox.SelectedItem is Ad_Statuses selectedStatus)
            {
                // Проверяем, является ли статус "Завершено" (или аналогичным)
                if (selectedStatus.status_name == "Завершено")
                {
                    ProfitPanel.Visibility = Visibility.Visible;

                    if (_adId.HasValue)
                    {
                        var ad = _context.Ads.Find(_adId.Value);
                    }
                }
                else
                {
                    ProfitPanel.Visibility = Visibility.Collapsed;
                    ProfitBox.Text = string.Empty;
                }
            }
            else
            {
                ProfitPanel.Visibility = Visibility.Collapsed;
                ProfitBox.Text = string.Empty;
            }
        }

        private bool ValidateInput(out decimal price, out decimal? finalPrice)
        {
            price = 0;
            finalPrice = null;

            if (string.IsNullOrWhiteSpace(TitleBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text) ||
                CityBox.SelectedItem == null ||
                CategoryBox.SelectedItem == null ||
                TypeBox.SelectedItem == null ||
                StatusBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PriceBox.Text.Replace('.', ','), out price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if ((StatusBox.SelectedItem as Ad_Statuses)?.status_name == "Завершено")
            {
                //поле "Полученная сумма"(только если статус "Завершено")
                if (string.IsNullOrWhiteSpace(ProfitBox.Text) && !_isAdAlreadyCompleted)
                {
                    MessageBox.Show("Для завершенного объявления необходимо указать полученную сумму.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                else
                {
                    // Если объявление только переводится в статус "Завершено"
                    decimal tempFinalPrice;
                    if (!decimal.TryParse(ProfitBox.Text.Replace('.', ','), out tempFinalPrice) || tempFinalPrice <= 0)
                    {
                        MessageBox.Show("Полученная сумма должна быть положительным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    finalPrice = tempFinalPrice;
                }
            }

            return true;
        }

        // Хелпер для переноса данных из формы в объект
        private void GetAdFromForm(Ads ad, double price)
        {
            ad.ad_title = TitleBox.Text.Trim();
            ad.ad_description = DescriptionBox.Text.Trim();
            ad.price = price;

            // Заполнение внешних ключей
            ad.city_id = (CityBox.SelectedItem as Cities).city_id;
            ad.category_id = (CategoryBox.SelectedItem as Categories).category_id;
            ad.ad_type_id = (TypeBox.SelectedItem as Ad_Types).type_id;
            ad.ad_status_id = (StatusBox.SelectedItem as Ad_Statuses).status_id;

            if (!string.IsNullOrWhiteSpace(_selectedPhotoPath))
            {
                ad.ad_photo_path = _selectedPhotoPath;
            }
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            // Используем стандартный диалог Windows для выбора файла
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите фотографию для объявления"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string targetFolder = System.IO.Path.Combine(appDirectory, "ФотоКарточек");

                    if (!System.IO.Directory.Exists(targetFolder))
                    {
                        System.IO.Directory.CreateDirectory(targetFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(openFileDialog.FileName);
                    string targetPath = System.IO.Path.Combine(targetFolder, fileName);

                    System.IO.File.Copy(openFileDialog.FileName, targetPath, true);

                    _selectedPhotoPath = $"/ФотоКарточек/{fileName}";

                    // 5. Показываем превью
                    AdImagePreview.Source = new BitmapImage(new Uri(targetPath));
                    PhotoTextBlock.Text = $"Фотография: {fileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке и сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // finalPrice будет иметь значение только при первом переводе в статус "Завершено"
            if (!ValidateInput(out decimal price, out decimal? finalPrice))
            {
                return;
            }

            try
            {
                Ads ad;
                bool isNewAd = !_adId.HasValue;
                bool statusChangedToCompleted = false;

                if (isNewAd)
                {
                    ad = new Ads();
                    ad.ad_post_date = DateTime.Now;
                    ad.user_id = SessionManager.CurrentUserId.Value;
                    _context.Ads.Add(ad);
                }
                else
                {
                    ad = _context.Ads.Find(_adId.Value);
                    if (ad == null)
                    {
                        MessageBox.Show("Объявление не найдено для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Обновление полей объявления
                GetAdFromForm(ad, (double)price);

                var newStatus = StatusBox.SelectedItem as Ad_Statuses;

                // Проверяем, что:
                // a) новый статус - "Завершено"
                // б) это не новое объявление (т.к. новое по умолчанию "Активно")
                // в) объявление *не было* завершено ранее (сравниваем с оригинальным статусом)
                if (newStatus?.status_name == "Завершено" && !isNewAd && _originalAdStatus?.status_name != "Завершено")
                {
                    statusChangedToCompleted = true;
                    var user = _context.Users.Find(ad.user_id);
                    if (user != null && finalPrice.HasValue)
                    {
                        // Обновление прибыли пользователя в таблице Users
                        user.profit = (int?)((user.profit ?? 0) + finalPrice.Value);
                        MessageBox.Show($"Прибыль пользователя {user.user_login} обновлена на {finalPrice.Value} руб.", "Прибыль обновлена", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                // 3. Сохранение изменений в БД
                _context.SaveChanges();

                MessageBox.Show(isNewAd ? "Объявление успешно добавлено!" : "Объявление успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
