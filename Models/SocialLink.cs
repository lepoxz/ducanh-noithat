using System;

namespace noithat_ducanh.Models
{
    public class SocialLink
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Facebook", "Zalo", "YouTube", "TikTok", "Instagram"
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // Stores the SVG XML string
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 0;
    }
}
