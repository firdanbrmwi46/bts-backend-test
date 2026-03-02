using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTS_Technical_Test.DTO
{
    public class ProductRequest
    {
        [Required(ErrorMessage = "Title wajib diisi")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price wajib diisi")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Category wajib diisi")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Images wajib diisi")]
        [MinLength(1, ErrorMessage = "Minimal harus ada 1 image")]
        public List<string> Images { get; set; } = new List<string>();
    }
}