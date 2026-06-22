using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages.Admin
{
    public class ManageCategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageCategoriesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        [BindProperty]
        public Category InputCategory { get; set; } = new();

        public bool IsEditMode { get; set; }

        public async Task OnGetAsync(int? editId)
        {
            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (editId.HasValue)
            {
                var category = await _context.Categories.FindAsync(editId.Value);
                if (category != null)
                {
                    InputCategory = category;
                    IsEditMode = true;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int? editId)
        {
            if (string.IsNullOrEmpty(InputCategory.Name))
            {
                ModelState.AddModelError("InputCategory.Name", "Tên danh mục không được để trống.");
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                IsEditMode = editId.HasValue;
                TempData["ToastMessage"] = "Tên danh mục là bắt buộc!";
                TempData["ToastType"] = "error";
                return Page();
            }

            try
            {
                string slug = GenerateSlug(InputCategory.Name);

                if (editId.HasValue) // Update Mode
                {
                    var category = await _context.Categories.FindAsync(editId.Value);
                    if (category != null)
                    {
                        var oldName = category.Name;
                        category.Name = InputCategory.Name;
                        category.Slug = slug;

                        // Synchronize any existing products that were under the old category string
                        var productsToUpdate = await _context.Products
                            .Where(p => p.Category == oldName)
                            .ToListAsync();

                        foreach (var p in productsToUpdate)
                        {
                            p.Category = InputCategory.Name;
                        }

                        await _context.SaveChangesAsync();
                        TempData["ToastMessage"] = $"Đã cập nhật danh mục: {category.Name}";
                        TempData["ToastType"] = "success";
                    }
                }
                else // Create Mode
                {
                    InputCategory.Slug = slug;
                    _context.Categories.Add(InputCategory);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã thêm danh mục mới: {InputCategory.Name}";
                    TempData["ToastType"] = "success";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                TempData["ToastType"] = "error";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category != null)
                {
                    var categoryName = category.Name;
                    _context.Categories.Remove(category);

                    // Update existing products under this category to "Chưa phân loại" so they aren't orphaned
                    var productsToUpdate = await _context.Products
                        .Where(p => p.Category == categoryName)
                        .ToListAsync();

                    foreach (var p in productsToUpdate)
                    {
                        p.Category = "Chưa phân loại";
                    }

                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã xóa danh mục: {categoryName}";
                    TempData["ToastType"] = "success";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Không thể xóa danh mục: {ex.Message}";
                TempData["ToastType"] = "error";
            }

            return RedirectToPage();
        }

        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            
            // Replace Vietnamese accents
            str = Regex.Replace(str, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");
            
            // Remove special characters and clean up spaces
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Replace(" ", "-");
            
            return str;
        }
    }
}
