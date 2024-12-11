using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using RestoranSiteV2.Models.Siniflar;
namespace RestoranSiteV2.Controllers
{
    public class KategoriController : Controller
    {
        // GET: Kategori
        Context c = new Context();
        public ActionResult Index()
        {
            var degerler = c.Kategoris.ToList();
            return View(degerler);
        }
        [HttpGet]
        public ActionResult KategoriEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult KategoriEkle(Kategori k)
        {
            c.Kategoris.Add(k);
            c.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult KategoriSil(int id)
        {
            var ktg = c.Kategoris.Find(id);
            c.Kategoris.Remove(ktg);
            c.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult KategoriGetir(int id)
        {
            var kategori = c.Kategoris.Find(id);
            return View("KategoriGetir", kategori);
        }
        public ActionResult KategoriGuncelle(Kategori k)
        {
            var ktgr = c.Kategoris.Find(k.KategoriID);
            ktgr.KategoriAd = k.KategoriAd;
            c.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DepartmanGuncelle(Departman p)
        {
            var dept = c.Departmans.Find(p.Departmanid);
            dept.DepartmanAd = p.DepartmanAd;
            c.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult ExcelExport()

        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var kategoriler = c.Kategoris.ToList(); // Sadece Durum = true olan ürünleri al
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Kategoriler");

                // Başlıkları ekleme
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Kategori Ad";
                

                // Verileri ekleme
                int row = 2;
                foreach (var kategori in kategoriler)
                {
                    worksheet.Cells[row, 1].Value = kategori.KategoriID;
                    worksheet.Cells[row, 2].Value = kategori.KategoriAd;
                   
                    row++;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Kategoriler.xlsx");
            }
        }
    }
}