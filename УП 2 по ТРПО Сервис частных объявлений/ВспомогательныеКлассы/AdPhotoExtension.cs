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
                if (string.IsNullOrWhiteSpace(this.ad_photo_path))
                {
                    return PlaceholderPath;
                }
                else
                {
                    return $"/ФотоКарточек/{this.ad_photo_path}";
                }
            }
        }
    }
}
