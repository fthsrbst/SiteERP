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
        [Authorize]
        public ActionResult Index()
        {
            // Toplam değerler
            ViewBag.TotalProducts = db.Uruns.Count();
            ViewBag.TotalCategories = db.Kategoris.Count();
            ViewBag.TotalDepartments = db.Departmans.Count();
            ViewBag.TotalPersonels = db.Personels.Count();

            // Kategoriye göre ürün sayıları
            var categoryData = db.Kategoris
                .Select(k => new
                {
                    CategoryName = k.KategoriAd,
                    ProductCount = k.Uruns.Count()
                })
                .ToList(); // LINQ işlemini tamamla
            ViewBag.CategoryDataJson = JsonConvert.SerializeObject(categoryData);

            // Aylık satış verileri
            var salesData = db.SatisHarekets
                .GroupBy(s => s.Tarih.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalSales = g.Sum(s => s.ToplamTutar)
                })
                .ToList();

            var monthNames = new Dictionary<int, string>
    {
        { 1, "Ocak" }, { 2, "Şubat" }, { 3, "Mart" }, { 4, "Nisan" },
        { 5, "Mayıs" }, { 6, "Haziran" }, { 7, "Temmuz" }, { 8, "Ağustos" },
        { 9, "Eylül" }, { 10, "Ekim" }, { 11, "Kasım" }, { 12, "Aralık" }
    };

            var completeSalesData = monthNames
                .Select(m => new
                {
                    MonthName = m.Value,
                    TotalSales = salesData.Where(s => s.Month == m.Key).Sum(s => s.TotalSales)
                })
                .ToList();

            ViewBag.SalesDataJson = JsonConvert.SerializeObject(completeSalesData);

            var productData = db.Uruns
                .GroupBy(u => DbFunctions.TruncateTime(u.EklenmeTarihi))
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.ProductDataJson = JsonConvert.SerializeObject(productData);

            // 1. En Çok Satılan Ürün
            var mostSoldProduct = db.SatisHarekets
                .GroupBy(s => s.Urunid)
                .Select(g => new
                {
                    UrunId = g.Key,
                    TotalSold = g.Sum(s => s.Adet)
                })
                .OrderByDescending(x => x.TotalSold)
                .FirstOrDefault();

            var mostSoldProductDetails = mostSoldProduct != null ? db.Uruns
                .Where(u => u.Urunid == mostSoldProduct.UrunId)
                .Select(u => new
                {
                    u.UrunAd,
                    TotalSold = mostSoldProduct.TotalSold
                })
                .FirstOrDefault() : null;

            var leastSoldProduct = db.SatisHarekets
                .GroupBy(s => s.Urunid)
                .Select(g => new
                {
                    UrunId = g.Key,
                    TotalSold = g.Sum(s => s.Adet)
                })
                .OrderBy(x => x.TotalSold)
                .FirstOrDefault();

            var leastSoldProductDetails = leastSoldProduct != null ? db.Uruns
                .Where(u => u.Urunid == leastSoldProduct.UrunId)
                .Select(u => new
                {
                    u.UrunAd,
                    TotalSold = leastSoldProduct.TotalSold
                })
                .FirstOrDefault() : null;

            // En Az Stoğa Sahip Ürün
            var leastStockProduct = db.Uruns
                .OrderBy(u => u.Stok)
                .FirstOrDefault();

            // Verileri ViewBag'e aktar
            ViewBag.MostSoldProductName = mostSoldProductDetails?.UrunAd ?? "Veri Yok";
            ViewBag.MostSoldQuantity = mostSoldProductDetails?.TotalSold ?? 0;

            ViewBag.LeastSoldProductName = leastSoldProductDetails?.UrunAd ?? "Veri Yok";
            ViewBag.LeastSoldQuantity = leastSoldProductDetails?.TotalSold ?? 0;

            ViewBag.LeastStockProductName = leastStockProduct?.UrunAd ?? "Veri Yok";
            ViewBag.LeastStockQuantity = leastStockProduct?.Stok ?? 0;

            // Ürün verilerini JSON formatında aktar
            ViewBag.Urunler = db.Uruns.Where(x => x.Durum == true).Select(u => new
            {
                u.Urunid,
                u.UrunAd,
                u.Stok,
                u.SatisFiyat
            }).ToList();

            return View();
        }

    }
}
