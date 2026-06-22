using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages.Admin
{
    public class ManageSocialLinksModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageSocialLinksModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SocialLink> SocialLinks { get; set; } = new();

        [BindProperty]
        public SocialLink InputSocialLink { get; set; } = new();

        public bool IsEditMode { get; set; }

        public async Task OnGetAsync(int? editId)
        {
            SocialLinks = await _context.SocialLinks
                .OrderBy(s => s.Order)
                .ToListAsync();

            if (editId.HasValue)
            {
                var link = await _context.SocialLinks.FindAsync(editId.Value);
                if (link != null)
                {
                    InputSocialLink = link;
                    IsEditMode = true;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int? editId)
        {
            if (string.IsNullOrEmpty(InputSocialLink.Name) || string.IsNullOrEmpty(InputSocialLink.Url))
            {
                ModelState.AddModelError("InputSocialLink.Name", "Tên và liên kết là bắt buộc.");
                SocialLinks = await _context.SocialLinks.OrderBy(s => s.Order).ToListAsync();
                IsEditMode = editId.HasValue;
                TempData["ToastMessage"] = "Vui lòng nhập đầy đủ thông tin bắt buộc!";
                TempData["ToastType"] = "error";
                return Page();
            }

            try
            {
                if (editId.HasValue) // Update
                {
                    var link = await _context.SocialLinks.FindAsync(editId.Value);
                    if (link != null)
                    {
                        link.Name = InputSocialLink.Name;
                        link.Url = InputSocialLink.Url;
                        link.Icon = InputSocialLink.Icon;
                        link.IsActive = InputSocialLink.IsActive;
                        link.Order = InputSocialLink.Order;

                        await _context.SaveChangesAsync();
                        TempData["ToastMessage"] = $"Đã cập nhật mạng xã hội: {link.Name}";
                        TempData["ToastType"] = "success";
                    }
                }
                else // Create
                {
                    _context.SocialLinks.Add(InputSocialLink);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã thêm mạng xã hội mới: {InputSocialLink.Name}";
                    TempData["ToastType"] = "success";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Lỗi: {ex.Message}";
                TempData["ToastType"] = "error";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var link = await _context.SocialLinks.FindAsync(id);
                if (link != null)
                {
                    var name = link.Name;
                    _context.SocialLinks.Remove(link);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã xóa liên kết: {name}";
                    TempData["ToastType"] = "success";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Lỗi khi xóa: {ex.Message}";
                TempData["ToastType"] = "error";
            }

            return RedirectToPage();
        }
    }
}
