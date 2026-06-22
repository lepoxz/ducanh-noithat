using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<ContactRequest> ContactRequests { get; set; } = new();
        public int ProductCount { get; set; }
        public int PostCount { get; set; }
        public int PendingContactCount { get; set; }

        public string HeroImage1 { get; set; } = string.Empty;
        public string HeroImage2 { get; set; } = string.Empty;
        public string HeroImage3 { get; set; } = string.Empty;

        public string Hotline { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            ContactRequests = await _context.ContactRequests
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ProductCount = await _context.Products.CountAsync();
            PostCount = await _context.Posts.CountAsync();
            PendingContactCount = await _context.ContactRequests.CountAsync(c => !c.IsProcessed);

            var settings = await _context.SystemSettings.ToListAsync();
            HeroImage1 = settings.FirstOrDefault(s => s.Key == "HeroImage1")?.Value ?? "/images/hero-kitchen.png";
            HeroImage2 = settings.FirstOrDefault(s => s.Key == "HeroImage2")?.Value ?? "/images/hero-bedroom.jpg";
            HeroImage3 = settings.FirstOrDefault(s => s.Key == "HeroImage3")?.Value ?? "/images/hero-kitchen2.jpg";

            Hotline = settings.FirstOrDefault(s => s.Key == "Hotline")?.Value ?? "0773 12 1234";
            Address = settings.FirstOrDefault(s => s.Key == "Address")?.Value ?? "Ông Bầu, Tỉnh Đồng Tháp";
            Email = settings.FirstOrDefault(s => s.Key == "Email")?.Value ?? "noithatnhua.ducanh@gmail.com";
        }

        public async Task<IActionResult> OnPostProcessAsync(int id)
        {
            var request = await _context.ContactRequests.FindAsync(id);
            if (request != null)
            {
                request.IsProcessed = true;
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = $"Đã xử lý yêu cầu của khách hàng: {request.FullName}";
                TempData["ToastType"] = "success";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var request = await _context.ContactRequests.FindAsync(id);
            if (request != null)
            {
                _context.ContactRequests.Remove(request);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Đã xóa yêu cầu tư vấn thành công.";
                TempData["ToastType"] = "success";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateSettingsAsync(
            string heroImage1, string heroImage2, string heroImage3,
            IFormFile? hero1File, IFormFile? hero2File, IFormFile? hero3File)
        {
            try
            {
                var settings = await _context.SystemSettings.ToListAsync();

                // Save hero 1 file if uploaded, otherwise use the text URL
                string val1 = heroImage1;
                if (hero1File != null && hero1File.Length > 0)
                {
                    string? uploadedPath = await SaveUploadedFileAsync(hero1File);
                    if (uploadedPath != null)
                    {
                        var oldVal = settings.FirstOrDefault(s => s.Key == "HeroImage1")?.Value;
                        DeleteOldFile(oldVal);
                        val1 = uploadedPath;
                    }
                }

                var h1 = settings.FirstOrDefault(s => s.Key == "HeroImage1");
                if (h1 != null) h1.Value = val1;
                else _context.SystemSettings.Add(new() { Key = "HeroImage1", Value = val1 });

                // Save hero 2 file if uploaded
                string val2 = heroImage2;
                if (hero2File != null && hero2File.Length > 0)
                {
                    string? uploadedPath = await SaveUploadedFileAsync(hero2File);
                    if (uploadedPath != null)
                    {
                        var oldVal = settings.FirstOrDefault(s => s.Key == "HeroImage2")?.Value;
                        DeleteOldFile(oldVal);
                        val2 = uploadedPath;
                    }
                }

                var h2 = settings.FirstOrDefault(s => s.Key == "HeroImage2");
                if (h2 != null) h2.Value = val2;
                else _context.SystemSettings.Add(new() { Key = "HeroImage2", Value = val2 });

                // Save hero 3 file if uploaded
                string val3 = heroImage3;
                if (hero3File != null && hero3File.Length > 0)
                {
                    string? uploadedPath = await SaveUploadedFileAsync(hero3File);
                    if (uploadedPath != null)
                    {
                        var oldVal = settings.FirstOrDefault(s => s.Key == "HeroImage3")?.Value;
                        DeleteOldFile(oldVal);
                        val3 = uploadedPath;
                    }
                }

                var h3 = settings.FirstOrDefault(s => s.Key == "HeroImage3");
                if (h3 != null) h3.Value = val3;
                else _context.SystemSettings.Add(new() { Key = "HeroImage3", Value = val3 });

                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Đã cập nhật ảnh nền Hero Slider thành công!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi khi cập nhật cài đặt: " + ex.Message;
                TempData["ToastType"] = "error";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateFooterSettingsAsync(string hotline, string address, string email)
        {
            try
            {
                var settings = await _context.SystemSettings.ToListAsync();

                var hlSetting = settings.FirstOrDefault(s => s.Key == "Hotline");
                if (hlSetting != null) hlSetting.Value = hotline;
                else _context.SystemSettings.Add(new() { Key = "Hotline", Value = hotline });

                var addrSetting = settings.FirstOrDefault(s => s.Key == "Address");
                if (addrSetting != null) addrSetting.Value = address;
                else _context.SystemSettings.Add(new() { Key = "Address", Value = address });

                var emailSetting = settings.FirstOrDefault(s => s.Key == "Email");
                if (emailSetting != null) emailSetting.Value = email;
                else _context.SystemSettings.Add(new() { Key = "Email", Value = email });

                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Đã cập nhật cấu hình thông tin liên hệ chân trang thành công!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi khi cập nhật cấu hình chân trang: " + ex.Message;
                TempData["ToastType"] = "error";
            }
            return RedirectToPage();
        }

        private async Task<string?> SaveUploadedFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Định dạng tệp không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG, WEBP, GIF.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("Dung lượng tệp vượt quá giới hạn 5MB.");
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        private void DeleteOldFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || !relativePath.StartsWith("/uploads/")) return;

            try
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
                // Suppress error
            }
        }
    }
}
