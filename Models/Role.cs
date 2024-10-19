using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThuCung.Models
{
    [Table("Role")]
    [PrimaryKey("idAdmin", "idRole")]
    public class Role
    {
        [Key, Column(Order = 0)]
        public int idAdmin { get; set; }
        [Key, Column(Order = 1)]
        public int idRole { get; set; }

        [ForeignKey("Mission")]
        public string idMission { get; set; }


        // Navigation Properties
        public Admin Admin { get; set; }
        public Mission Mission { get; set; }
    }
}
