using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            // If already logged in, redirect to Admin Index
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Admin/Index");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/Admin");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/Admin");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Find user in DB
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username.ToLower() == Input.Username.ToLower());

            if (admin != null)
            {
                var hasher = new PasswordHasher<noithat_ducanh.Models.Admin>();
                var result = hasher.VerifyHashedPassword(admin, admin.PasswordHash, Input.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    // Sign in
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.Username),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("AdminId", admin.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        ExpiresUtc = Input.RememberMe 
                            ? DateTimeOffset.UtcNow.AddDays(7) 
                            : DateTimeOffset.UtcNow.AddHours(2)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Update last login time
                    admin.LastLogin = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return LocalRedirect(ReturnUrl);
                }
            }

            // If we got this far, something failed
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            return Page();
        }
    }
}
