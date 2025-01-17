using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestoranSiteV2.Models.Siniflar;

namespace RestoranSiteV2.Controllers
{
    [Authorize]
    public class UrunController : Controller
    {
        Context c = new Context();

        public ActionResult Index(string marka, int? kategoriId, DateTime? startDate, DateTime? endDate,
                          int? minStock, int? maxStock, decimal? minAlisFiyat, decimal? maxAlisFiyat,
                          decimal? minSatisFiyat, decimal? maxSatisFiyat)
        {
            // Tüm ürünleri çek
            var urunler = c.Uruns.Include("Kategori").Where(x => x.Durum == true).AsQueryable();

            // Filtreleme işlemleri
            if (!string.IsNullOrEmpty(marka))
            {
                urunler = urunler.Where(x => x.Marka.Contains(marka));
            }

            if (kategoriId.HasValue)
            {
                urunler = urunler.Where(x => x.Kategoriid == kategoriId.Value);
            }

            if (startDate.HasValue)
            {
                urunler = urunler.Where(x => x.EklenmeTarihi >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                urunler = urunler.Where(x => x.EklenmeTarihi <= endDate.Value);
            }

            if (minStock.HasValue)
            {
                urunler = urunler.Where(x => x.Stok >= minStock.Value);
            }

            if (maxStock.HasValue)
            {
                urunler = urunler.Where(x => x.Stok <= maxStock.Value);
            }

            if (minAlisFiyat.HasValue)
            {
                urunler = urunler.Where(x => x.AlisFiyat >= minAlisFiyat.Value);
            }

            if (maxAlisFiyat.HasValue)
            {
                urunler = urunler.Where(x => x.AlisFiyat <= maxAlisFiyat.Value);
            }

            if (minSatisFiyat.HasValue)
            {
                urunler = urunler.Where(x => x.SatisFiyat >= minSatisFiyat.Value);
            }

            if (maxSatisFiyat.HasValue)
            {
                urunler = urunler.Where(x => x.SatisFiyat <= maxSatisFiyat.Value);
            }

            // Grafik için verileri ViewBag'e aktar
            ViewBag.Urunler = c.Uruns.Where(x => x.Durum == true).Select(u => new
            {
                u.Urunid, // Ürün ID
                u.UrunAd,
                u.Stok,
                u.SatisFiyat
            }).ToList();


            // Filtrelenmiş listeyi gönder
            ViewBag.Kategoriler = c.Kategoris.ToList(); // Kategoriler dropdown için
            return View(urunler.ToList());
        }

        [HttpGet]
        public ActionResult YeniUrun()
        {
            List<SelectListItem> deger1 = (from x in c.Kategoris.ToList()
                                           select new SelectListItem
                                           {
                                               Text = x.KategoriAd,
                                               Value = x.KategoriID.ToString()
                                           }).ToList();
            ViewBag.dgr1 = deger1;
            return View();
        }

        [HttpPost]
        public ActionResult YeniUrun(Urun p)
        {
            if (ModelState.IsValid)
            {
                c.Uruns.Add(p);
                c.SaveChanges();
                TempData["AlertMessage"] = "Ürün başarıyla kaydedildi.";
                return RedirectToAction("Index");
            }

            return View(p);
        }

        public ActionResult UrunSil(int id)
        {
            var deger = c.Uruns.Find(id);
            deger.Durum = false;
            c.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Detay(int id)
        {
            var urun = c.Uruns.Include("Kategori").FirstOrDefault(x => x.Urunid == id);
            if (urun == null)
            {
                return HttpNotFound();
            }
            return View(urun);
        }
        [HttpPost]
        public ActionResult AciklamaEkle(int Urunid, string Aciklama)
        {
            var urun = c.Uruns.FirstOrDefault(x => x.Urunid == Urunid);
            if (urun != null)
            {
                urun.Aciklama = Aciklama;
                c.SaveChanges();
            }
            return RedirectToAction("Detay", new { id = Urunid });
        }

        [HttpPost]
        public JsonResult UrunGuncelle(Urun model)
        {
            // Modeldeki Urunid'yi logla
            System.Diagnostics.Debug.WriteLine("Model Urunid: " + model.Urunid);

            // Veritabanında ürünü sorgula
            var urun = c.Uruns.Where(x => x.Urunid == model.Urunid).FirstOrDefault();

            // Veritabanındaki Urunid'yi logla
            System.Diagnostics.Debug.WriteLine("Veritabanındaki Urunid: " + (urun != null ? urun.Urunid.ToString() : "Ürün bulunamadı"));

            if (urun != null)
            {
                // Ürünü güncelle
                urun.UrunAd = model.UrunAd;
                urun.Marka = model.Marka;
                urun.Stok = model.Stok;
                urun.AlisFiyat = model.AlisFiyat;
                urun.SatisFiyat = model.SatisFiyat;
                urun.UrunGorsel = model.UrunGorsel;
                urun.Kategoriid = model.Kategoriid;
                urun.Aciklama = model.Aciklama;
                urun.Durum = model.Durum;

                // Değişiklikleri kaydet
                c.SaveChanges();

                return Json(new { success = true, message = "Ürün başarıyla güncellendi." });
            }

            return Json(new { success = false, message = "Ürün bulunamadı." });
        }






        // Controller'da UrunGetir methodu
        public ActionResult UrunGetir(int id)
        {
            // Gelen id'yi kontrol için loglayın
            System.Diagnostics.Debug.WriteLine("Alınan ID: " + id);

            var urun = c.Uruns
                .Where(u => u.Urunid == id)
                .Select(u => new
                {
                    Urunid = u.Urunid,
                    UrunAd = u.UrunAd,
                    Marka = u.Marka,
                    Stok = u.Stok,
                    AlisFiyat = u.AlisFiyat,
                    SatisFiyat = u.SatisFiyat,
                    UrunGorsel = u.UrunGorsel,
                    Kategoriid = u.Kategoriid,
                    Aciklama = u.Aciklama,
                    Durum = u.Durum
                })
                .FirstOrDefault();

            if (urun == null)
            {
                System.Diagnostics.Debug.WriteLine("Ürün bulunamadı."); // Log ekleyin
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            System.Diagnostics.Debug.WriteLine("Bulunan Ürün: " + urun.UrunAd); // Log ekleyin
            return Json(urun, JsonRequestBehavior.AllowGet);
        }








        public ActionResult ExcelExport()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var urunler = c.Uruns.Where(x => x.Durum == true).ToList();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Urunler");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Ürün";
                worksheet.Cells[1, 3].Value = "Marka";
                worksheet.Cells[1, 4].Value = "Stok";
                worksheet.Cells[1, 5].Value = "Alış Fiyatı";
                worksheet.Cells[1, 6].Value = "Satış Fiyatı";
                worksheet.Cells[1, 7].Value = "Kategori";
                worksheet.Cells[1, 8].Value = "Görsel";

                int row = 2;
                foreach (var urun in urunler)
                {
                    worksheet.Cells[row, 1].Value = urun.Urunid;
                    worksheet.Cells[row, 2].Value = urun.UrunAd;
                    worksheet.Cells[row, 3].Value = urun.Marka;
                    worksheet.Cells[row, 4].Value = urun.Stok;
                    worksheet.Cells[row, 5].Value = urun.AlisFiyat;
                    worksheet.Cells[row, 6].Value = urun.SatisFiyat;
                    worksheet.Cells[row, 7].Value = urun.Kategori.KategoriAd;
                    worksheet.Cells[row, 8].Value = urun.UrunGorsel;
                    row++;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                TempData["excelMessage"] = "Ürünler.xlsx başarıyla indirildi.";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Urunler.xlsx");
            }
        }

        [HttpPost]
        public ActionResult ExcelImport(HttpPostedFileBase file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Ticari kullanımı olmayanlar için

            if (file != null && file.ContentLength > 0)
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return Json(new { success = false, message = "Excel dosyası boş veya okunamadı." });
                    }

                    // Start from the second row to skip the header row
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var urunAd = worksheet.Cells[row, 1].Text;
                        var marka = worksheet.Cells[row, 2].Text;
                        var stok = 0;
                        var alisFiyat = 0m;
                        var satisFiyat = 0m;
                        var kategoriAd = worksheet.Cells[row, 6].Text;

                        // Safely parse values to avoid errors if data is malformed
                        if (int.TryParse(worksheet.Cells[row, 3].Text, out stok) &&
                            decimal.TryParse(worksheet.Cells[row, 4].Text, out alisFiyat) &&
                            decimal.TryParse(worksheet.Cells[row, 5].Text, out satisFiyat))
                        {
                            var kategori = c.Kategoris.FirstOrDefault(k => k.KategoriAd == kategoriAd);
                            if (kategori != null)
                            {
                                Urun urun = new Urun
                                {
                                    UrunAd = urunAd,
                                    Marka = marka,
                                    Stok = (short)stok,
                                    AlisFiyat = alisFiyat,
                                    SatisFiyat = satisFiyat,
                                    Kategoriid = kategori.KategoriID,
                                    Durum = true
                                };

                                c.Uruns.Add(urun);
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Excel dosyasındaki bazı veriler hatalı." });
                        }
                    }

                    c.SaveChanges();
                    return Json(new { success = true, message = "Excel dosyasındaki ürünler başarıyla eklendi." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Lütfen bir Excel dosyası yükleyin." });
            }
        }



    }


}