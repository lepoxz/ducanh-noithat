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
    public class ProjectsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProjectsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Project> Projects { get; set; } = new();

        public async Task OnGetAsync()
        {
            Projects = await _context.Projects
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }
    }
}
