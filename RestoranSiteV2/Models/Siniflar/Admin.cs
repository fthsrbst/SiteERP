using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RestoranSiteV2.Models.Siniflar
{
    public class Admin
    {
        [Key]
        public int Adminid { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(10)]
        public string KullaniciAd { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(255)] // Hash şifre daha uzun olacak
        public string Sifre { get; set; }

        [Column(TypeName = "Char")]
        [StringLength(1)]
        public string Yetki { get; set; }

        // Şifreyi hash yapmak için yardımcı bir metod ekliyoruz
        public void SetPasswordHash(string password)
        {
            var passwordHasher = new PasswordHasher<Admin>();
            Sifre = passwordHasher.HashPassword(this, password);
        }

        // Şifre doğrulama metodu
        public bool VerifyPassword(string password)
        {
            var passwordHasher = new PasswordHasher<Admin>();
            var result = passwordHasher.VerifyHashedPassword(this, Sifre, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
