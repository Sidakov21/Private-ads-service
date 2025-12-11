using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace УП_2_по_ТРПО_Сервис_частных_объявлений
{
    public partial class Ads
    {
        // Константа для пути к заглушке
        private const string PlaceholderPath = "/Фото/Заглушка.jpg";

        public string DisplayPhotoSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.ad_photo_path))
                {
                    return PlaceholderPath;
                }
                else
                {
                    try
                    {
                        string relativePathFromDb = this.ad_photo_path.TrimStart('/', '\\');
                        string finalRelativePath;

                        // Проверяем, содержит ли путь имя папки 'ФотоКарточек'.
                        if (!relativePathFromDb.StartsWith("ФотоКарточек", StringComparison.OrdinalIgnoreCase))
                        {
                            finalRelativePath = Path.Combine("ФотоКарточек", relativePathFromDb);
                        }
                        else
                        {
                            finalRelativePath = relativePathFromDb;
                        }

                        // Получаем полный путь к файлу на диске (относительно EXE)
                        string fullPath = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            finalRelativePath
                        );

                        // Опциональная проверка: если файла нет, показываем заглушку
                        if (!File.Exists(fullPath))
                        {
                            return PlaceholderPath;
                        }

                        // Преобразуем его в абсолютный URI для WPF Image Source
                        return new Uri(fullPath).AbsoluteUri;
                    }
                    catch
                    {
                        // В случае ошибки возвращаем заглушку
                        return PlaceholderPath;
                    }
                }
            }
        }
    }
}
