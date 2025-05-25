using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    [Table("ReportStudents")]
    public class ReportStudents
    {
        [Key]
        public int report_student_id { get; set; }
        public int report_id { get; set; }
        public long student_code { get; set; }
    }
}
