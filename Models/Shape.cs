using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThuCung.Models
{
    [Table("Shape")]
    public class Shape
    {
        [Key]
        public string idShape { get; set; }

        [ForeignKey("SanPham")]
        public string idProduct { get; set; }

        [MaxLength(100)]
        public string Image { get; set; }


        // Navigation Property
        public Product Product { get; set; }
    }
}
