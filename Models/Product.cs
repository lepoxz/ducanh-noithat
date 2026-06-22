using System;

namespace noithat_ducanh.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Category { get; set; } = string.Empty; // e.g., "Tủ bếp", "Tủ quần áo", "Giường ngủ", "Bàn ghế", "Văn phòng"
        public string VideoUrls { get; set; } = string.Empty; // Newline-separated YouTube video links
        public string InstallationImageUrls { get; set; } = string.Empty; // Newline-separated installation image URLs
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<ProductComparison> Comparisons { get; set; } = new();
    }
}
