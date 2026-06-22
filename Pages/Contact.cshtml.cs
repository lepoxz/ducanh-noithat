using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ContactModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContactRequest Contact { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(Contact.FullName) || string.IsNullOrEmpty(Contact.PhoneNumber))
            {
                ModelState.AddModelError("Contact.FullName", "Họ tên và Số điện thoại là bắt buộc.");
                return Page();
            }

            Contact.CreatedAt = DateTime.Now;
            Contact.IsProcessed = false;

            _context.ContactRequests.Add(Contact);
            await _context.SaveChangesAsync();

            SuccessMessage = "Gửi thông tin liên hệ thành công! Chúng tôi sẽ phản hồi trong vòng 24h làm việc.";

            // Clear the form fields after successful submit
            Contact = new ContactRequest();

            return Page();
        }
    }
}
