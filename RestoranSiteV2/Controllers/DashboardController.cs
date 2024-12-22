using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization; // Ay isimleri için
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using RestoranSiteV2.Models.Siniflar;

namespace RestoranSiteV2.Controllers
{
    public class DashboardController : Controller
    {
        private readonly Context db = new Context();

        public ActionResult Index()
        {
            // Toplam değerler
            ViewBag.TotalProducts = db.Uruns.Count();
            ViewBag.TotalCategories = db.Kategoris.Count();
            ViewBag.TotalDepartments = db.Departmans.Count();

            // Kategoriye göre ürün sayıları
            var categoryData = db.Kategoris
                .Select(k => new
                {
                    CategoryName = k.KategoriAd,
                    ProductCount = k.Uruns.Count()
                })
                .ToList(); // LINQ işlemini tamamla
            ViewBag.CategoryDataJson = JsonConvert.SerializeObject(categoryData);

            // Tüm ay isimlerini hazırla
            var monthNames = new Dictionary<int, string>
    {
        { 1, "Ocak" }, { 2, "Şubat" }, { 3, "Mart" }, { 4, "Nisan" },
        { 5, "Mayıs" }, { 6, "Haziran" }, { 7, "Temmuz" }, { 8, "Ağustos" },
        { 9, "Eylül" }, { 10, "Ekim" }, { 11, "Kasım" }, { 12, "Aralık" }
    };

            // Aylık satış verileri
            var salesData = db.SatisHarekets
                .GroupBy(s => s.Tarih.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalSales = g.Sum(s => s.ToplamTutar)
                })
                .ToList();

            // Eksik ayları tamamla
            var completeSalesData = monthNames
                .Select(m => new
                {
                    MonthName = m.Value,
                    TotalSales = salesData.Where(s => s.Month == m.Key).Sum(s => s.TotalSales) // Eğer satış yoksa 0
                })
                .ToList();

            ViewBag.SalesDataJson = JsonConvert.SerializeObject(completeSalesData);

            var productData = db.Uruns
                .GroupBy(u => DbFunctions.TruncateTime(u.EklenmeTarihi))  // Sadece tarih kısmını almak için DbFunctions.TruncateTime kullanıyoruz
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date) // Tarihe göre sıralama
                .ToList();

            // JSON verisini ViewBag'e aktar
            ViewBag.ProductDataJson = JsonConvert.SerializeObject(productData);

            return View();
        }


    }
}
