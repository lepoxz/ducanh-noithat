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
    public class NewsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public NewsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Post> Posts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Posts = await _context.Posts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
