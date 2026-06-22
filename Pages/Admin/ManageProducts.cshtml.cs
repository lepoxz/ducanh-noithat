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
    public class ManageProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManageProductsModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty]
        public Product InputProduct { get; set; } = new();

        [BindProperty]
        public IFormFile? ProductImageFile { get; set; }

        [BindProperty]
        public List<IFormFile> InstallationImageFiles { get; set; } = new();

        [BindProperty]
        public IFormFile? ComparisonBeforeFile { get; set; }

        [BindProperty]
        public IFormFile? ComparisonAfterFile { get; set; }

        [BindProperty]
        public string ComparisonDescription { get; set; } = string.Empty;

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
            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var lowerSearch = SearchQuery.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || 
                                         p.Category.ToLower().Contains(lowerSearch) || 
                                         (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
            }

            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Products = await query
                .OrderByDescending(p => p.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            if (editId.HasValue)
            {
                var product = await _context.Products
                    .Include(p => p.Comparisons)
                    .FirstOrDefaultAsync(p => p.Id == editId.Value);
                if (product != null)
                {
                    InputProduct = product;
                    IsEditMode = true;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int? editId)
        {
            if (string.IsNullOrEmpty(InputProduct.Name) || string.IsNullOrEmpty(InputProduct.Category))
            {
                ModelState.AddModelError("InputProduct.Name", "Tên sản phẩm và Danh mục là bắt buộc.");
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                
                var query = _context.Products.AsQueryable();
                if (!string.IsNullOrEmpty(SearchQuery))
                {
                    var lowerSearch = SearchQuery.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || 
                                             p.Category.ToLower().Contains(lowerSearch) || 
                                             (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
                }
                TotalItems = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

                Products = await query
                    .OrderByDescending(p => p.Id)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                IsEditMode = editId.HasValue;
                TempData["ToastMessage"] = "Tên sản phẩm và Danh mục không được để trống!";
                TempData["ToastType"] = "error";
                return Page();
            }

            // Handle image file upload
            if (ProductImageFile != null && ProductImageFile.Length > 0)
            {
                try
                {
                    string? uploadedPath = await SaveUploadedFileAsync(ProductImageFile);
                    if (uploadedPath != null)
                    {
                        if (editId.HasValue)
                        {
                            var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editId.Value);
                            if (oldProduct != null && !string.IsNullOrEmpty(oldProduct.ImageUrl))
                            {
                                DeleteOldFile(oldProduct.ImageUrl);
                            }
                        }
                        InputProduct.ImageUrl = uploadedPath;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProductImageFile", ex.Message);
                    Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                    
                    var query = _context.Products.AsQueryable();
                    if (!string.IsNullOrEmpty(SearchQuery))
                    {
                        var lowerSearch = SearchQuery.ToLower();
                        query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || 
                                                 p.Category.ToLower().Contains(lowerSearch) || 
                                                 (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
                    }
                    TotalItems = await query.CountAsync();
                    TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                    if (PageNumber < 1) PageNumber = 1;
                    if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

                    Products = await query
                        .OrderByDescending(p => p.Id)
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    IsEditMode = editId.HasValue;
                    TempData["ToastMessage"] = "Lỗi khi tải ảnh lên: " + ex.Message;
                    TempData["ToastType"] = "error";
                    return Page();
                }
            }

            // Handle multiple installation images upload
            var newInstallationImages = new List<string>();
            if (InstallationImageFiles != null && InstallationImageFiles.Count > 0)
            {
                try
                {
                    foreach (var file in InstallationImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string? uploadedPath = await SaveUploadedFileAsync(file);
                            if (uploadedPath != null)
                            {
                                newInstallationImages.Add(uploadedPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Clean up any successfully uploaded files in this request if one of them fails
                    foreach (var path in newInstallationImages)
                    {
                        DeleteOldFile(path);
                    }

                    ModelState.AddModelError("InstallationImageFiles", ex.Message);
                    Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                    
                    var query = _context.Products.AsQueryable();
                    if (!string.IsNullOrEmpty(SearchQuery))
                    {
                        var lowerSearch = SearchQuery.ToLower();
                        query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || 
                                                 p.Category.ToLower().Contains(lowerSearch) || 
                                                 (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
                    }
                    TotalItems = await query.CountAsync();
                    TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                    if (PageNumber < 1) PageNumber = 1;
                    if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

                    Products = await query
                        .OrderByDescending(p => p.Id)
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    IsEditMode = editId.HasValue;
                    TempData["ToastMessage"] = "Lỗi khi tải ảnh thi công: " + ex.Message;
                    TempData["ToastType"] = "error";
                    return Page();
                }
            }

            if (editId.HasValue) // Update Mode
            {
                var product = await _context.Products.FindAsync(editId.Value);
                if (product != null)
                {
                    product.Name = InputProduct.Name;
                    product.Description = InputProduct.Description;
                    
                    if (!string.IsNullOrEmpty(InputProduct.ImageUrl))
                    {
                        product.ImageUrl = InputProduct.ImageUrl;
                    }
                    else if (ProductImageFile == null)
                    {
                        product.ImageUrl = InputProduct.ImageUrl;
                    }

                    product.Price = InputProduct.Price;
                    product.OldPrice = InputProduct.OldPrice;
                    product.Category = InputProduct.Category;
                    product.IsActive = InputProduct.IsActive;

                    // Update video and installation images
                    product.VideoUrls = InputProduct.VideoUrls ?? string.Empty;

                    var existingImagesList = new List<string>();
                    if (!string.IsNullOrEmpty(product.InstallationImageUrls))
                    {
                        existingImagesList.AddRange(product.InstallationImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    existingImagesList.AddRange(newInstallationImages);
                    product.InstallationImageUrls = string.Join("\n", existingImagesList);

                    // Save Before/After comparison images if uploaded
                    if (ComparisonBeforeFile != null && ComparisonBeforeFile.Length > 0 && 
                        ComparisonAfterFile != null && ComparisonAfterFile.Length > 0)
                    {
                        try
                        {
                            string? beforeUrl = await SaveUploadedFileAsync(ComparisonBeforeFile);
                            string? afterUrl = await SaveUploadedFileAsync(ComparisonAfterFile);
                            if (beforeUrl != null && afterUrl != null)
                            {
                                var comparison = new ProductComparison
                                {
                                    ProductId = product.Id,
                                    BeforeImageUrl = beforeUrl,
                                    AfterImageUrl = afterUrl,
                                    Description = ComparisonDescription ?? string.Empty,
                                    CreatedAt = DateTime.Now
                                };
                                _context.ProductComparisons.Add(comparison);
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["ToastMessage"] = "Lỗi khi tải cặp ảnh so sánh: " + ex.Message;
                            TempData["ToastType"] = "error";
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã cập nhật sản phẩm: {product.Name}";
                    TempData["ToastType"] = "success";
                }
            }
            else // Create Mode
            {
                InputProduct.CreatedAt = DateTime.Now;
                InputProduct.InstallationImageUrls = string.Join("\n", newInstallationImages);
                _context.Products.Add(InputProduct);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = $"Đã thêm sản phẩm mới: {InputProduct.Name}";
                TempData["ToastType"] = "success";
            }

            return RedirectToPage(new { SearchQuery = SearchQuery, PageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Comparisons)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteOldFile(product.ImageUrl);
                }
                
                if (!string.IsNullOrEmpty(product.InstallationImageUrls))
                {
                    var images = product.InstallationImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var img in images)
                    {
                        DeleteOldFile(img);
                    }
                }

                // Delete physical comparison files
                foreach (var comp in product.Comparisons)
                {
                    DeleteOldFile(comp.BeforeImageUrl);
                    DeleteOldFile(comp.AfterImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Đã xóa sản phẩm thành công.";
                TempData["ToastType"] = "success";
            }
            return RedirectToPage(new { SearchQuery = SearchQuery, PageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteInstallationImageAsync(int productId, string imageUrl)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null && !string.IsNullOrEmpty(product.InstallationImageUrls))
            {
                var images = product.InstallationImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (images.Contains(imageUrl))
                {
                    images.Remove(imageUrl);
                    product.InstallationImageUrls = string.Join("\n", images);
                    await _context.SaveChangesAsync();
                    DeleteOldFile(imageUrl);
                    TempData["ToastMessage"] = "Đã xóa hình ảnh thi công!";
                    TempData["ToastType"] = "success";
                }
            }
            return RedirectToPage(new { editId = productId, SearchQuery = SearchQuery, PageNumber = PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteComparisonAsync(int comparisonId, int productId)
        {
            var comparison = await _context.ProductComparisons.FindAsync(comparisonId);
            if (comparison != null)
            {
                DeleteOldFile(comparison.BeforeImageUrl);
                DeleteOldFile(comparison.AfterImageUrl);
                _context.ProductComparisons.Remove(comparison);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Đã xóa cặp ảnh so sánh thành công!";
                TempData["ToastType"] = "success";
            }
            return RedirectToPage(new { editId = productId, SearchQuery = SearchQuery, PageNumber = PageNumber });
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
