using System.ComponentModel.DataAnnotations;

namespace ProyectoCamisetas.Models
{
    public class HomeCarouselSlide
    {
        [Range(1, 3)]
        public short Orden { get; set; }

        [StringLength(2048)]
        public string? ImageUrl { get; set; }

        [StringLength(120)]
        public string? Title { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(40)]
        public string? ButtonText { get; set; }

        [StringLength(512)]
        public string? ButtonUrl { get; set; }
    }

    public class HomeCarouselConfig
    {
        [StringLength(2048)]
        public string? Slide1Url { get; set; }

        [StringLength(2048)]
        public string? Slide2Url { get; set; }

        [StringLength(2048)]
        public string? Slide3Url { get; set; }

        [StringLength(120)]
        public string? Title { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(40)]
        public string? ButtonText { get; set; }

        [StringLength(512)]
        public string? ButtonUrl { get; set; }
    }
}

