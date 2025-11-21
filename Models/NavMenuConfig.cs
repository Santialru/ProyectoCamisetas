using System.Collections.Generic;

namespace ProyectoCamisetas.Models
{
    public class NavMenuConfig
    {
        public List<NavSection> Sections { get; set; } = new();
    }

    public class NavSection
    {
        public string? Title { get; set; }
        public short Orden { get; set; }
        public List<NavLinkConfig> Links { get; set; } = new();
    }

    public class NavLinkConfig
    {
        public string? Label { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public short Orden { get; set; }
        public string? Href { get; set; }
    }
}
