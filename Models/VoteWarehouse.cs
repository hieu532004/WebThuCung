using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThuCung.Models
{
    [Table("VoteWarehouse")]
    public class VoteWarehouse
    {
        [Key]
        public string idVotewarehouse { get; set; }

        [Required]
        public DateTime dateEntry { get; set; }
        [ForeignKey("Supplier")]
        public string idSupplier { get; set; }


        [Required]
        public int Quantity { get; set; }

        public decimal? totalVoteWarehouse { get; set; }
        public Supplier Supplier {  get; set; }

        // Navigation Property
        public ICollection<DetailVoteWarehouse> DetailVoteWarehouses { get; set; }

    }
}
