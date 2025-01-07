using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
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
        // Login Post (POST)
        // Login Post (POST)
        // Login Post (POST)
        [HttpPost]
        public ActionResult Login(Admin model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins.FirstOrDefault(a => a.KullaniciAd == model.KullaniciAd);
                if (admin != null && VerifyPassword(model.Sifre, admin.Sifre)) // Hash doğrulaması
                {
                    // Başarılı giriş, cookie oluşturuluyor
                    var cookie = new HttpCookie("AuthCookie")
                    {
                        Value = $"{admin.KullaniciAd}|{DateTime.Now.ToString()}",
                        Expires = DateTime.Now.AddHours(1), // 1 saatlik süre
                        HttpOnly = true, // XSS saldırılarına karşı güvenlik
                        Secure = Request.IsSecureConnection // Sadece HTTPS üzerinden gönderilmesini sağla
                    };
                    Response.Cookies.Add(cookie);

                    // Başarılı giriş kontrolü
                    if (Request.Cookies["AuthCookie"] != null)
                    {
                        Console.WriteLine("Cookie oluşturuldu: " + Request.Cookies["AuthCookie"].Value);
                    }

                    // Dashboard'a yönlendir
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




        public ActionResult Logout()
        {
            var authCookie = Request.Cookies["AuthCookie"];
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.Now.AddDays(-1); // Cookie'yi geçersiz kıl
                authCookie.Value = null;                      // Değerini sıfırla
                authCookie.Path = "/";
                authCookie.HttpOnly = true;                   // Güvenlik için HttpOnly
                authCookie.Secure = Request.IsSecureConnection; // HTTPS için Secure flag
                Response.Cookies.Set(authCookie);

                System.Diagnostics.Debug.WriteLine("Logout: Cookie başarıyla silindi.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Logout: AuthCookie bulunamadı.");
            }

            // Kullanıcıyı login sayfasına yönlendir
            return RedirectToAction("Login", "Account");
        }


        // Şifre Unuttum Post (AJAX)
        [HttpPost]
        public JsonResult ForgotPassword(string username)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.KullaniciAd == username);
            System.Diagnostics.Debug.WriteLine($"Kullanıcı adı arandı: {username}");

            if (admin != null)
            {
                var verificationCode = GenerateRandomCode();
                Session["VerificationCode"] = verificationCode; // Doğrulama kodunu oturuma kaydet
                Session["ResetUser"] = admin.KullaniciAd;

                // Konsola doğrulama kodunu yazdır
                System.Diagnostics.Debug.WriteLine(verificationCode);

                // Doğrulama kodunu tarayıcıya gönder
                return Json(new { success = true, verificationCode = verificationCode });
            }
            return Json(new { success = false });
        }


        // Şifre Sıfırlama Post (AJAX)
        [HttpPost]
        public JsonResult ResetPassword(string verificationCode, string newPassword)
        {
            // Debug çıktısını kontrol etmek için
            System.Diagnostics.Debug.WriteLine($"Entered ResetPassword method: {verificationCode}, {newPassword}");

            if (Session["VerificationCode"]?.ToString() == verificationCode && Session["ResetUser"] != null)
            {
                var username = Session["ResetUser"].ToString();
                System.Diagnostics.Debug.WriteLine($"ResetUser: {username}");

                var admin = _context.Admins.FirstOrDefault(a => a.KullaniciAd == username);
                if (admin != null)
                {
                    admin.Sifre = HashPassword(newPassword); // Şifreyi hash'le
                    _context.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Password reset successful for user: {username}");

                    // Doğrulama kodu ve kullanıcı bilgisini temizle
                    Session.Remove("VerificationCode");
                    Session.Remove("ResetUser");

                    return Json(new { success = true });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Admin user not found for username: {username}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Verification failed or ResetUser session is null.");
            }

            return Json(new { success = false });
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

        // Hash kontrolü
        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == hashedPassword;
        }

        // Rastgele doğrulama kodu oluşturma
        private string GenerateRandomCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
