using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int user_id { get; set; }
        public string? full_name { get; set; }
        public string? email { get; set; }
        public string? phone_number { get; set; }
        public string? gender { get; set; }
        public DateTime date_of_birth { get; set; }
        public string? desired_role { get; set; }
        public string? password { get; set; }
        public Boolean is_active { get; set; }
        public int role_id { get; set; }
    }
}
