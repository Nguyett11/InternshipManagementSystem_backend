using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    [Table("Roles")]
    public class Roles
    {
        [Key]
        public int role_id { get; set; }
        public string? name { get; set; }
    }
}
