using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    [Table("Lecturers")]
    public class Lecturers
    {
        [Key]
        public int lecturer_id { get; set; }
        public string lecturer_name { get; set; }
        public string department { get; set; }
        public int user_id { get; set; }
    }
}
