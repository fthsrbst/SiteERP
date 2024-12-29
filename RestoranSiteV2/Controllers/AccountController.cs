using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using RestoranSiteV2.Models.Siniflar;

namespace RestoranSiteV2.Controllers
{
    public class AccountController : Controller
    {
        private readonly Context _context; // Context sınıfı ile bağlantı

        public AccountController()
        {
            _context = new Context(); // Context sınıfını başlatıyoruz
        }

        // Login Page (GET)
        public ActionResult Login()
        {
            var model = new Admin(); // Modeli başlat
            return View(model); // View'a gönder
        }

        // Login Post (POST)
        [HttpPost]
        public ActionResult Login(Admin model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins.FirstOrDefault(a => a.KullaniciAd == model.KullaniciAd);
                if (admin != null && admin.Sifre == model.Sifre) // Şifre kontrolü
                {
                    // Başarılı giriş, Dashboard'a yönlendir
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    // Kullanıcı adı veya şifre yanlış, hata mesajı ekle
                    ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                }
            }
            return View(model); // Hata varsa, giriş sayfasını tekrar göster
        }

        // Şifre Unuttum Page (GET)
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // Şifre Unuttum Post (POST)
        [HttpPost]
        public JsonResult ForgotPassword(string email)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.KullaniciAd == email);
            if (admin != null)
            {
                var resetCode = GenerateRandomCode();
                TempData["ResetCode"] = resetCode;
                // Gerçek uygulamada e-posta gönderme işlemi yapılabilir
                return Json(new { success = true, resetCode = resetCode });
            }
            else
            {
                return Json(new { success = false });
            }
        }


        // Şifre Sıfırlama Page (GET)
        public ActionResult ResetPassword()
        {
            return View();
        }

        // Şifre Sıfırlama Post (POST)
        [HttpPost]
        public ActionResult ResetPassword(string resetCode, string newPassword)
        {
            if (resetCode == TempData["ResetCode"]?.ToString())
            {
                var admin = _context.Admins.FirstOrDefault();
                if (admin != null)
                {
                    // Şifreyi hash'leme işlemi
                    admin.Sifre = HashPassword(newPassword); // Hashleme işlemi
                    _context.SaveChanges(); // Yeni şifreyi veritabanına kaydet
                    return RedirectToAction("Login");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz reset kodu.");
            }
            return View();
        }

        // Şifreyi hash'leme fonksiyonu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }


        // Rastgele şifre sıfırlama kodu oluşturma
        private string GenerateRandomCode()
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            return code;
        }
    }
}
