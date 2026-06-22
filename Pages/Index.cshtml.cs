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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Product> FamilyProducts { get; set; } = new();
        public List<Product> OfficeProducts { get; set; } = new();
        public List<Post> Posts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Project> Projects { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty]
        public ContactRequest Contact { get; set; } = new();

        public string HeroImage1 { get; set; } = string.Empty;
        public string HeroImage2 { get; set; } = string.Empty;
        public string HeroImage3 { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Seed data if empty
            await SeedDataAsync();

            // Fetch News/Posts
            Posts = await _context.Posts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            // Fetch Categories
            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Fetch Projects
            Projects = await _context.Projects
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            // Fetch Settings for Hero
            var settings = await _context.SystemSettings.ToListAsync();
            HeroImage1 = settings.FirstOrDefault(s => s.Key == "HeroImage1")?.Value ?? "/images/hero-kitchen.png";
            HeroImage2 = settings.FirstOrDefault(s => s.Key == "HeroImage2")?.Value ?? "/images/hero-bedroom.jpg";
            HeroImage3 = settings.FirstOrDefault(s => s.Key == "HeroImage3")?.Value ?? "/images/hero-kitchen2.jpg";

            // Fetch Products based on filtering
            var query = _context.Products.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var lowerSearch = SearchQuery.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || p.Description.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(p => p.Category == Category);
            }

            var allProducts = await query.ToListAsync();

            // Segment products into Family and Office categories
            // Family categories: Tủ bếp, Tủ quần áo, Giường ngủ, Phòng ngủ, Bàn ghế (excluding Office)
            FamilyProducts = allProducts.Where(p => p.Category != "Văn phòng").ToList();
            OfficeProducts = allProducts.Where(p => p.Category == "Văn phòng").ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Contact.FullName) || string.IsNullOrEmpty(Contact.PhoneNumber))
            {
                ModelState.AddModelError("Contact.FullName", "Họ tên và Số điện thoại là bắt buộc.");
                // Reload data
                await OnGetAsync();
                return Page();
            }

            Contact.CreatedAt = DateTime.Now;
            Contact.IsProcessed = false;

            _context.ContactRequests.Add(Contact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gửi yêu cầu tư vấn thành công! Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất.";

            return RedirectToPage("/Index");
        }

        private async Task SeedDataAsync()
        {
            if (await _context.Products.AnyAsync() || await _context.Posts.AnyAsync())
            {
                return; // Seeded already
            }

            // Products
            var products = new List<Product>
            {
                new() {
                    Name = "Mẫu Nội Thất Phòng Ngủ Nhựa Cao Cấp Đa Năng Hiện Đại – PN180",
                    Category = "Phòng ngủ",
                    Description = "Bộ nội thất phòng ngủ nhựa Đài Loan cao cấp đa năng, thiết kế sang trọng, hiện đại, tối ưu diện tích.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/mau-noi-that-phong-ngu-nhua-cao-cap-thiet-ke-da-nang-hien-dai-pn180.jpg",
                    Price = null
                },
                new() {
                    Name = "Mẫu Nội Thất Phòng Ngủ Nhựa Cao Cấp Đa Năng Vân Gỗ – PN126",
                    Category = "Phòng ngủ",
                    Description = "Nội thất phòng ngủ vân gỗ ấm cúng, tinh tế, chất liệu nhựa cao cấp chống ẩm, chống mối mọt tuyệt đối.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/mau-noi-that-phong-ngu-nhua-cao-cap-da-nang-van-go-pn126.jpg",
                    Price = null
                },
                new() {
                    Name = "Tủ Bếp Nhựa Cao Cấp Phủ Acrylic Chữ I Hiện Đại – TB176",
                    Category = "Tủ bếp",
                    Description = "Tủ bếp nhựa phủ Acrylic bóng gương sang trọng, chống nước 100%, thiết kế chữ I nhỏ gọn.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/tu-bep-nhua-cao-cap-phu-acrylic-chu-i-hien-dai-tb176.jpg",
                    Price = null
                },
                new() {
                    Name = "Tủ Bếp Nhựa Giả Gỗ Cao Cấp Chữ L Tiết Kiệm Không Gian – TB084",
                    Category = "Tủ bếp",
                    Description = "Tủ bếp nhựa giả gỗ ấm cúng, thiết kế chữ L tối ưu hóa góc phòng bếp, tiện nghi cao.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/tu-bep-nhua-gia-go-cao-cap-chu-l-tiet-kiem-khong-gian-tb084.jpg",
                    Price = null
                },
                new() {
                    Name = "Tủ Bếp Nhựa Chung Cư Cao Cấp Màu Xám – TB076",
                    Category = "Tủ bếp",
                    Description = "Mẫu tủ bếp nhựa màu xám hiện đại, trẻ trung, rất được ưa chuộng tại các căn hộ chung cư.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/tu-bep-nhua-chung-cu-cao-cap-mau-xam-tb076.jpg",
                    Price = null
                },
                new() {
                    Name = "Tủ Bếp Nhựa Cao Cấp – TB030",
                    Category = "Tủ bếp",
                    Description = "Tủ bếp nhựa Đài Loan cao cấp, bền bỉ, dễ dàng lau chùi vệ sinh.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/tu-bep-nhua-cao-cap-tb030.jpg",
                    Price = null
                },
                new() {
                    Name = "Tủ Quần Áo Nhựa Đài Loan Cao Cấp – TQA029",
                    Category = "Tủ quần áo",
                    Description = "Tủ quần áo nhựa Đài Loan 4 cánh rộng rãi, chia nhiều ngăn treo đồ tiện lợi.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/tu-quan-ao-nhua-dai-loan-cao-cap-tqa029.jpg",
                    Price = null
                },
                new() {
                    Name = "Tủ Quần Áo Nhựa Đài Loan Cao Cấp – TQA009",
                    Category = "Tủ quần áo",
                    Description = "Tủ quần áo nhựa nhỏ gọn cho phòng ngủ nhỏ, bền đẹp, đa dạng màu sắc.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/tu-quan-ao-nhua-dai-loan-cao-cap-tqa009.jpg",
                    Price = null
                },
                new() {
                    Name = "Nội Thất Văn Phòng VP10",
                    Category = "Văn phòng",
                    Description = "Bàn ghế làm việc văn phòng nhựa cao cấp, thiết kế hiện đại, chống mối mọt.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/noi-that-van-phong-vp10.jpg",
                    Price = 2500000,
                    OldPrice = 3050000
                },
                new() {
                    Name = "Nội Thất Văn Phòng VP9",
                    Category = "Văn phòng",
                    Description = "Hệ tủ hồ sơ và bàn làm việc văn phòng sang trọng.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/noi-that-van-phong-vp9.jpg",
                    Price = 9250000,
                    OldPrice = 12500000
                },
                new() {
                    Name = "Bàn Ăn 6 Ghế Bingo – Walnut",
                    Category = "Bàn ghế",
                    Description = "Bộ bàn ăn gỗ kết hợp nhựa sang trọng màu Walnut ấm áp cho gia đình.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/ban-an-6-ghe-bingo-walnut.jpg",
                    Price = 7640000,
                    OldPrice = 7900000
                },
                new() {
                    Name = "Bàn Ghế Ăn Cabin Bộ 4 Ghế – Antique",
                    Category = "Bàn ghế",
                    Description = "Bộ bàn ghế ăn cabin 4 ghế nhỏ gọn, tinh tế cho phòng ăn chung cư.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/product/ban-ghe-an-cabin-bo-4-ghe-antique.jpg",
                    Price = 3300000,
                    OldPrice = 3790000
                }
            };

            await _context.Products.AddRangeAsync(products);

            // Posts
            var posts = new List<Post>
            {
                new() {
                    Title = "Địa Điểm Mua Nội Thất Nhựa Uy Tín Tại Hà Nội",
                    Summary = "Tìm nơi mua nội thất nhựa tại Hà Nội uy tín? Phân tích chi tiết vật liệu, ưu nhược điểm và lý do nên chọn Nội Thất Đức Anh.",
                    Content = "Tìm nơi mua nội thất nhựa tại Hà Nội uy tín? Phân tích chi tiết vật liệu, ưu nhược điểm và lý do nên chọn Nội Thất Đức Anh.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/news/dia-diem-mua-noi-that-nhua-uy-tin-tai-ha-noi.jpg"
                },
                new() {
                    Title = "Bật Mí Xu Hướng Màu Sắc Tủ Bếp Được Ưa Chuộng Năm 2026",
                    Summary = "Khi lựa chọn tủ bếp, màu sắc là yếu tố được quan tâm rất nhiều. Cùng khám phá những xu hướng màu sắc hot nhất năm nay.",
                    Content = "Khi lựa chọn tủ bếp, màu sắc là yếu tố được quan tâm rất nhiều. Cùng khám phá những xu hướng màu sắc hot nhất năm nay.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/news/bat-mi-xu-huong-mau-sac-tu-bep.jpg"
                },
                new() {
                    Title = "Xu Hướng Thiết Kế Tủ Bếp Hiện Đại Dẫn Đầu Năm 2026",
                    Summary = "Nội thất nhà bếp luôn là mối quan tâm lớn. Thiết kế tủ bếp hiện đại, đầy đủ công năng với giá thành hợp lý.",
                    Content = "Nội thất nhà bếp luôn là mối quan tâm lớn. Thiết kế tủ bếp hiện đại, đầy đủ công năng với giá thành hợp lý.",
                    ImageUrl = "https://noithathaiminh.com.vn/uploads/news/xu-huong-thiet-ke-tu-bep-hien-dai.jpg"
                }
            };

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();
        }
    }
}
