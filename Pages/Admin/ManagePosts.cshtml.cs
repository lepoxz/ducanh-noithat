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
    public class ManagePostsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManagePostsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Post> Posts { get; set; } = new();

        [BindProperty]
        public Post InputPost { get; set; } = new();

        public bool IsEditMode { get; set; }

        public async Task OnGetAsync(int? editId)
        {
            Posts = await _context.Posts
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            if (editId.HasValue)
            {
                var post = await _context.Posts.FindAsync(editId.Value);
                if (post != null)
                {
                    InputPost = post;
                    IsEditMode = true;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int? editId)
        {
            if (string.IsNullOrEmpty(InputPost.Title) || string.IsNullOrEmpty(InputPost.Content))
            {
                ModelState.AddModelError("InputPost.Title", "Tiêu đề và Nội dung bài viết không được để trống.");
                Posts = await _context.Posts.OrderByDescending(p => p.Id).ToListAsync();
                IsEditMode = editId.HasValue;
                TempData["ToastMessage"] = "Tiêu đề và nội dung bài viết là bắt buộc!";
                TempData["ToastType"] = "error";
                return Page();
            }

            try
            {
                if (editId.HasValue) // Update Mode
                {
                    var post = await _context.Posts.FindAsync(editId.Value);
                    if (post != null)
                    {
                        post.Title = InputPost.Title;
                        post.Summary = InputPost.Summary;
                        post.Content = InputPost.Content;
                        post.ImageUrl = InputPost.ImageUrl;
                        post.IsActive = InputPost.IsActive;

                        await _context.SaveChangesAsync();
                        TempData["ToastMessage"] = $"Đã cập nhật bài viết: {post.Title}";
                        TempData["ToastType"] = "success";
                    }
                    else
                    {
                        TempData["ToastMessage"] = "Không tìm thấy bài viết để cập nhật.";
                        TempData["ToastType"] = "error";
                    }
                }
                else // Create Mode
                {
                    InputPost.CreatedAt = DateTime.Now;
                    _context.Posts.Add(InputPost);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = $"Đã đăng bài viết mới: {InputPost.Title}";
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
                var post = await _context.Posts.FindAsync(id);
                if (post != null)
                {
                    _context.Posts.Remove(post);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Đã xóa bài viết thành công.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Không tìm thấy bài viết để xóa.";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Không thể xóa bài viết: {ex.Message}";
                TempData["ToastType"] = "error";
            }
            
            return RedirectToPage();
        }
    }
}
