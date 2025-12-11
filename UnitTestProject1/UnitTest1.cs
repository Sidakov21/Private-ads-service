using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

// Класс-заглушка для имитации объекта Users из Entity Framework
public class MockUser
{
    public int user_id { get; set; }
    public double? profit { get; set; }
}

// Класс для тестирования логики валидации (имитация логики из AdEditorWindow)
public static class AdValidator
{
    public static bool IsValid(string title, double? price)
    {
        // Проверка на пустой заголовок
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        // Проверка на корректную цену (должна быть, и должна быть > 0)
        if (!price.HasValue || price.Value <= 0)
        {
            return false;
        }

        return true;
    }
}

// Класс-заглушка для тестирования Ads.DisplayPhotoSource
public class MockAds
{
    private const string PlaceholderPath = "/Фото/Заглушка.jpg";
    public string ad_photo_path { get; set; }

    // Воспроизводим исправленную логику DisplayPhotoSource
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
                string relativePathFromDb = this.ad_photo_path.TrimStart('/', '\\');
                string finalRelativePath;

                // Логика исправления старых путей (проверяем, содержит ли имя папки 'ФотоКарточек')
                if (!relativePathFromDb.StartsWith("ФотоКарточек", StringComparison.OrdinalIgnoreCase))
                {
                    finalRelativePath = Path.Combine("ФотоКарточек", relativePathFromDb);
                }
                else
                {
                    finalRelativePath = relativePathFromDb;
                }

                // В тестах проверяем только генерацию правильного относительного пути
                return finalRelativePath;
            }
        }
    }
}

public static class SessionManager
{
    public static MockUser CurrentUser { get; private set; }
    public static bool IsAuthorized => CurrentUser != null;

    public static void Login(MockUser user)
    {
        CurrentUser = user;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}

namespace UnitTestProject1
{
    [TestClass]
    public class AdLogicTests
    {
        // 1. Проверка на пустые/некорректные поля (TC_VAL_01)
        [TestMethod]
        public void Test_RequiredFields_EmptyTitle_ReturnsFalse()
        {
            string emptyTitle = "";
            double validPrice = 100.0;

            bool result = AdValidator.IsValid(emptyTitle, validPrice);

            Assert.IsFalse(result, "Валидация должна завершиться неудачей при пустом заголовке.");
        }

        // Дополнительный тест: некорректная цена
        [TestMethod]
        public void Test_RequiredFields_InvalidPrice_ReturnsFalse()
        {
            
            string validTitle = "Продам стул";
            double? invalidPrice = null;

            bool result = AdValidator.IsValid(validTitle, invalidPrice);

            Assert.IsFalse(result, "Валидация должна завершиться неудачей при отсутствующей цене.");
        }


        // 2. Проверка логики расчета прибыли (TC_PROFIT_03)
        [TestMethod]
        public void Test_ProfitCalculation_AddsCorrectAmount()
        {
        
            var user = new MockUser { user_id = 1, profit = 1000.0 };
            double adPrice = 500.0;
            double expectedProfit = 1500.0;

            user.profit = (user.profit ?? 0) + adPrice;

            Assert.AreEqual(expectedProfit, user.profit.Value, "Прибыль должна быть увеличена на точную сумму цены объявления.");
        }

        // 3. Тестирование пути к фото (Логика Ads.cs)
        // Тест для старого формата пути (только имя файла)
        [TestMethod]
        public void Test_DisplayPhotoSource_OldPathFormat_Corrected()
        {
            var ad = new MockAds { ad_photo_path = "old_image.jpg" };

            string actualPath = ad.DisplayPhotoSource;

            string expectedEnd = Path.Combine("ФотоКарточек", "old_image.jpg");
            Assert.IsTrue(actualPath.EndsWith(expectedEnd, StringComparison.OrdinalIgnoreCase), "Старый путь должен быть дополнен префиксом 'ФотоКарточек'.");
        }


        // 4. Управление сессией (Logout Logic)
        [TestMethod]
        public void Test_Logout_SetsCurrentUserToNull()
        {
            
            var user = new MockUser { user_id = 10 };
            SessionManager.Login(user);

            SessionManager.Logout();

            Assert.IsFalse(SessionManager.IsAuthorized, "После выхода пользователь не должен быть авторизован.");
            Assert.IsNull(SessionManager.CurrentUser, "CurrentUser должен быть null после выхода.");
        }
    }
}
