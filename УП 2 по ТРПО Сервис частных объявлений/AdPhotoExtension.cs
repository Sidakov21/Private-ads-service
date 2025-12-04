using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                // 1. Проверяем значение поля из БД (ad_photo_path)
                // Если путь пуст, NULL или состоит только из пробелов
                if (string.IsNullOrWhiteSpace(this.ad_photo_path))
                {
                    // 2. Возвращаем путь к заглушке
                    return PlaceholderPath;
                }
                else
                {
                    // 3. Возвращаем путь к реальной фотографии.
                    // Предполагается, что в БД хранится ТОЛЬКО ИМЯ ФАЙЛА (например, "image1.jpg")
                    // и все файлы лежат в папке /ФотоКарточек/
                    return $"/ФотоКарточек/{this.ad_photo_path}";

                    // Если в БД уже хранится полный относительный путь (например, "/Фото/image1.jpg"), 
                    // то используйте просто: return this.ad_photo_path;
                }
            }
        }
    }
}
