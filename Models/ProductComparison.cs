using System;

namespace noithat_ducanh.Models
{
    public class ProductComparison
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string BeforeImageUrl { get; set; } = string.Empty;
        public string AfterImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public Product? Product { get; set; }
    }
}
