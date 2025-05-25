using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    [Table("Grading")]
    public class Grading
    {
        [Key]
        public int id { get; set; }
        public int submission_id { get; set; }
        public int lecturer_id { get; set; }
        public string grade { get; set; }
        public string feedback { get; set; }
    }
}
