using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThuCung.Models
{
    [Table("Mission")]
    public class Mission
    {
        [Key, MaxLength(100)]
        public string idMission { get; set; }

        [MaxLength(200)]
        public string nameMission { get; set; }

        // Navigation Property
        public ICollection<Role> Roles { get; set; }
    }
}
