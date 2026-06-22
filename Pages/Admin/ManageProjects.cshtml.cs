using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages.Admin
{
    public class ManageProjectsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManageProjectsModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<Project> Projects { get; set; } = new();

        [BindProperty]
        public Project InputProject { get; set; } = new();

        [BindProperty]
        public IFormFile? ProjectImageFile { get; set; }

        [BindProperty]
        public List<IFormFile> ExtraImageFiles { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public bool IsEditMode { get; set; }

        public async Task OnGetAsync(int? editId)
        {
            var query = _context.Projects.AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var lowerSearch = SearchQuery.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(lowerSearch) ||
                                         p.Category.ToLower().Contains(lowerSearch) ||
                                         p.Location.ToLower().Contains(lowerSearch) ||
                                         (p.Summary != null && p.Summary.ToLower().Contains(lowerSearch)));
            }

            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Projects = await query
                .OrderByDescending(p => p.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            if (editId.HasValue)
            {
                var project = await _context.Projects.FindAsync(editId.Value);
                if (project != null)
                {
                    InputProject = project;
                    IsEditMode = true;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int? editId)
        {
            if (string.IsNullOrEmpty(InputProject.Title) || string.IsNullOrEmpty(InputProject.Category))
            {
                ModelState.AddModelError("InputProject.Title", "Tiêu đề và Loại dự án là bắt buộc.");
                return await ReloadPageAsync(editId.HasValue);
            }

            // Handle main cover image upload
            if (ProjectImageFile != null && ProjectImageFile.Length > 0)
            {
                try
                {
                    string? uploadedPath = await SaveUploadedFileAsync(ProjectImageFile);
                    if (uploadedPath != null)
                    {
                        if (editId.HasValue)
                        {
                            var oldProject = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editId.Value);
                            if (oldProject != null && !string.IsNullOrEmpty(oldProject.ImageUrl))
                            {
                                DeleteOldFile(oldProject.ImageUrl);
                            }
                        }
                        InputProject.ImageUrl = uploadedPath;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProjectImageFile", ex.Message);
                    return await ReloadPageAsync(editId.HasValue);
                }
            }

            // Handle multiple extra project images upload
            var newExtraImages = new List<string>();
            if (ExtraImageFiles != null && ExtraImageFiles.Count > 0)
            {
                try
                {
                    foreach (var file in ExtraImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string? uploadedPath = await SaveUploadedFileAsync(file);
                            if (uploadedPath != null)
                            {
                                newExtraImages.Add(uploadedPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    foreach (var path in newExtraImages)
                    {
                        DeleteOldFile(path);
                    }
                    ModelState.AddModelError("ExtraImageFiles", ex.Message);
                    return await ReloadPageAsync(editId.HasValue);
                }
            }

            if (editId.HasValue) // Update Mode
            {
                var project = await _context.Projects.FindAsync(editId.Value);
                if (project != null)
                {
                    project.Title = InputProject.Title;
                    project.Category = InputProject.Category;
                    project.Summary = InputProject.Summary;
                    project.Content = InputProject.Content ?? string.Empty;
                    project.Location = InputProject.Location;
                    project.CustomerName = InputProject.CustomerName;
                    project.CompletionDate = InputProject.CompletionDate;
                    project.IsActive = InputProject.IsActive;

                    if (!string.IsNullOrEmpty(InputProject.ImageUrl))
                    {
                        project.ImageUrl = InputProject.ImageUrl;
                    }

                    // Append new extra image urls
                    var existingImages = new List<string>();
                    if (!string.IsNullOrEmpty(project.ProjectImageUrls))
                    {
                        existingImages.AddRange(project.ProjectImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    existingImages.AddRange(newExtraImages);
                    project.ProjectImageUrls = string.Join("\n", existingImages);

                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã cập nhật dự án: {project.Title}";
                    TempData["ToastType"] = "success";
                }
            }
            else // Create Mode
            {
                InputProject.CreatedAt = DateTime.Now;
                InputProject.ProjectImageUrls = string.Join("\n", newExtraImages);
                if (string.IsNullOrEmpty(InputProject.ImageUrl))
                {
                    InputProject.ImageUrl = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&q=80"; // Default fallback
                }
                _context.Projects.Add(InputProject);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = $"Đã tạo dự án mới: {InputProject.Title}";
                TempData["ToastType"] = "success";
            }

            return RedirectToPage(new { SearchQuery = SearchQuery, PageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                if (!string.IsNullOrEmpty(project.ImageUrl))
                {
                    DeleteOldFile(project.ImageUrl);
                }

                if (!string.IsNullOrEmpty(project.ProjectImageUrls))
                {
                    var images = project.ProjectImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var img in images)
                    {
                        DeleteOldFile(img);
                    }
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Đã xóa dự án thành công.";
                TempData["ToastType"] = "success";
            }
            return RedirectToPage(new { SearchQuery = SearchQuery, PageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteProjectImageAsync(int projectId, string imageUrl)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null && !string.IsNullOrEmpty(project.ProjectImageUrls))
            {
                var images = project.ProjectImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (images.Contains(imageUrl))
                {
                    images.Remove(imageUrl);
                    project.ProjectImageUrls = string.Join("\n", images);
                    await _context.SaveChangesAsync();
                    DeleteOldFile(imageUrl);
                    TempData["ToastMessage"] = "Đã xóa ảnh thi công phụ khỏi dự án!";
                    TempData["ToastType"] = "success";
                }
            }
            return RedirectToPage(new { editId = projectId, SearchQuery = SearchQuery, PageNumber = PageNumber });
        }

        private async Task<IActionResult> ReloadPageAsync(bool isEdit)
        {
            var query = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var lowerSearch = SearchQuery.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(lowerSearch) ||
                                         p.Category.ToLower().Contains(lowerSearch) ||
                                         p.Location.ToLower().Contains(lowerSearch));
            }

            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            Projects = await query
                .OrderByDescending(p => p.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            IsEditMode = isEdit;
            TempData["ToastMessage"] = "Vui lòng kiểm tra lại các trường nhập liệu!";
            TempData["ToastType"] = "error";
            return Page();
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
