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
    public class NewsDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public NewsDetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Post Post { get; set; } = null!;
        public List<Post> RecentPosts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (post == null)
            {
                return NotFound();
            }

            Post = post;

            // Fetch other 3 recent active posts for recommendation
            RecentPosts = await _context.Posts
                .Where(p => p.Id != Post.Id && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            return Page();
        }
    }
}
