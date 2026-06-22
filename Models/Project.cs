using System;

namespace noithat_ducanh.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // e.g., "Chung cư", "Biệt thự", "Nhà phố", "Văn phòng"
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
        public string ProjectImageUrls { get; set; } = string.Empty; // Newline-separated URLs
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
