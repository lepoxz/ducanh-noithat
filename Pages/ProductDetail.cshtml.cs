using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages
{
    public class ProductDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductDetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; } = null!;
        public List<Product> RelatedProducts { get; set; } = new();
        public List<string> VideoIds { get; set; } = new();
        public List<string> InstallationImages { get; set; } = new();
 
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Comparisons)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null)
            {
                return NotFound();
            }
 
            Product = product;
 
            // Parse video urls and installation images
            if (!string.IsNullOrEmpty(Product.VideoUrls))
            {
                var urls = Product.VideoUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var url in urls)
                {
                    var videoId = GetYouTubeId(url);
                    if (!string.IsNullOrEmpty(videoId))
                    {
                        VideoIds.Add(videoId);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Product.InstallationImageUrls))
            {
                InstallationImages = Product.InstallationImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // Fetch related products (same category, excluding this one)
            RelatedProducts = await _context.Products
                .Where(p => p.Category == Product.Category && p.Id != Product.Id && p.IsActive)
                .Take(4)
                .ToListAsync();
 
            return Page();
        }

        private string GetYouTubeId(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            url = url.Trim();
            if (url.Contains("youtu.be/"))
            {
                return url.Split("youtu.be/").Last().Split('?').First().Split('&').First();
            }
            if (url.Contains("v="))
            {
                return url.Split("v=").Last().Split('&').First().Split('?').First();
            }
            if (url.Contains("embed/"))
            {
                return url.Split("embed/").Last().Split('?').First().Split('&').First();
            }
            return string.Empty;
        }
    }
}
