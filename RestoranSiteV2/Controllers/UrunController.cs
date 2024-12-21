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
    public class UrunController : Controller
    {
        Context c = new Context();

        public ActionResult Index()
        {

            var urunler = c.Uruns.Where(x => x.Durum == true).ToList();

            // Populate ViewBag.Kategoriler
            ViewBag.Kategoriler = c.Kategoris.ToList(); // Assuming 'Kategoriler' is the correct collection

            return View(urunler);
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
        public ActionResult UrunGuncelle(Urun p)
        {
            if (p == null || p.Urunid == 0)
            {
                return Json(new { success = false, message = "Geçersiz ürün bilgisi." });
            }

            var urn = c.Uruns.Find(p.Urunid);
            if (urn == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            try
            {
                // Güncellemeler
                urn.UrunAd = p.UrunAd;
                urn.Marka = p.Marka;
                urn.AlisFiyat = p.AlisFiyat;
                urn.SatisFiyat = p.SatisFiyat;
                urn.Stok = p.Stok;
                urn.UrunGorsel = p.UrunGorsel;
                urn.Kategoriid = p.Kategoriid;

                c.SaveChanges();
                return Json(new { success = true, message = "Ürün başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
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