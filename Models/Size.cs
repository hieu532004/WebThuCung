using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThuCung.Models
{
    [Table("Size")]
    public class Size
    {
        [Key]
        public string idSize { get; set; }

        [ForeignKey("Product")]
        public string idProduct { get; set; }

        public int nameSize { get; set; }
        // Navigation Property
        public Product Product { get; set; }
    }
}
