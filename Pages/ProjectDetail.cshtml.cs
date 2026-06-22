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
    public class ProjectDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProjectDetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Project Project { get; set; } = null!;
        public List<Project> OtherProjects { get; set; } = new();
        public List<string> ProjectImages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (project == null)
            {
                return NotFound();
            }

            Project = project;

            if (!string.IsNullOrEmpty(Project.ProjectImageUrls))
            {
                ProjectImages = Project.ProjectImageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            OtherProjects = await _context.Projects
                .Where(p => p.Id != Project.Id && p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToListAsync();

            return Page();
        }
    }
}
