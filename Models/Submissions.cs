using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    [Table("Submissions")]
    public class Submissions
    {
        [Key]
        public int submission_id { get; set; }
        public int report_id { get; set; }
        public long student_code { get; set; }
        public string file { get; set; }
        public DateTime submission_date { get; set; }
        public string status { get; set; }
    }
}
