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
    public class ProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 8;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToListAsync();

            var query = _context.Products.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(p => p.Category == Category);
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var lowerSearch = SearchQuery.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || 
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
        }
    }
}
