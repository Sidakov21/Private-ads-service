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
using System.Windows.Shapes;
using УП_2_по_ТРПО_Сервис_частных_объявлений.ВспомогательныеКлассы;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new Sidakov_DB_PrivateAdsEntities())
            {
                // Находим пользователя по логину и паролю
                var user = db.Users.FirstOrDefault(u =>
                    u.user_login == login &&
                    u.user_password == password
                );

                if (user != null)
                {
                    // Сохраняем ID пользователя в статический класс для доступа из других окон
                    SessionManager.SetUser((int)user.user_id);

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordBox.Password = string.Empty;
                }
            }
        }
    }
}
