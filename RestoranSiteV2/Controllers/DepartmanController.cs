using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestoranSiteV2.Models.Siniflar;
namespace RestoranSiteV2.Controllers
{
    public class DepartmanController : Controller
    {
        // GET: Departman
        Context c = new Context();
        [Authorize]
        public ActionResult Index()
        {
            var degerler = c.Departmans.Where(x=>x.Durum==true).ToList();
            return View(degerler);
        }
        [HttpGet]
        public ActionResult DepartmanEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult YeniDepartman(Departman p)
        {
            if (ModelState.IsValid)
            {
                c.Departmans.Add(p);
                c.SaveChanges();
                TempData["AlertMessage"] = "Departman başarıyla kaydedildi.";
                return RedirectToAction("Index");
            }

            return View(p);
        }



        public ActionResult DepartmanSil(int id)
        {
            var dep = c.Departmans.Find(id);
            dep.Durum = false;
            c.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DepartmanGetir(int id)
        {
            var dpt = c.Departmans.Find(id);
            return View("DepartmanGetir", dpt);
        }
        public ActionResult DepartmanGuncelle(Departman p)
        {
            var dept = c.Departmans.Find(p.Departmanid);
            dept.DepartmanAd = p.DepartmanAd;
            c.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DepartmanDetay(int id)
        {
            var degerler=c.Personels.Where(x => x.Departmanid == id).ToList();
            var dpt = c.Departmans.Where(x => x.Departmanid == id).Select(y => y.DepartmanAd).FirstOrDefault();
            ViewBag.d = dpt;
            return View(degerler);
        }
        public ActionResult DepartmanPersonelSatis(int id)
        {
            var degerler = c.SatisHarekets.Where(x => x.Personelid == id).ToList();
            var per = c.Personels.Where(x => x.Personelid == id).Select(y => y.PersonelAd + " " + y.PersonelSoyad).FirstOrDefault();
            ViewBag.dpers = per;
            return View(degerler);
            
        }
        public JsonResult GetDepartmanPersonelSayilari()
        {
            var departmanPersonelSayilari = c.Departmans
                                             .Select(d => new
                                             {
                                                 departmanAd = d.DepartmanAd,
                                                 personelSayisi = d.Personels.Count
                                             })
                                             .ToList();

            return Json(departmanPersonelSayilari, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartmanPersonelleri(int departmanId)
        {
            var personelListesi = c.Personels
                                  .Where(p => p.Departmanid == departmanId)
                                  .Select(p => new
                                  {
                                      p.Personelid,
                                      p.PersonelAd,
                                      p.PersonelSoyad,
                                      p.PersonelGorsel
                                  })
                                  .ToList();

            return Json(personelListesi, JsonRequestBehavior.AllowGet);
        }



    }
}