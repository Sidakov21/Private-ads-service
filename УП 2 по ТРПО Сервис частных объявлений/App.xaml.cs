using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Скрываем стандартное главное окно, если оно есть
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AuthorizationWindow loginWindow = new AuthorizationWindow();
            bool? dialogResult = loginWindow.ShowDialog();

            if (dialogResult == true)
            {
                MainWindow mainWindow = new MainWindow();
                this.MainWindow = mainWindow;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
                loginWindow.Close();
            }
            else
            {
                this.Shutdown();
            }
        }
    }
}
