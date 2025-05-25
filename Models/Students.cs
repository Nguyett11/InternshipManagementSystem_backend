using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    [Table("Students")]
    public class Students
    {
        [Key]
        public long student_code { get; set; }
        public string student_name { get; set; }
        public string class_student { get; set; }
        public string major { get; set; }
        public int year_of_study { get; set; }
        public int company_id { get; set; }
        public int mentor_id { get; set; }
        public int lecturer_id { get; set; }
        public DateOnly start_date { get; set; }
        public DateOnly end_date { get; set; }
        public string language { get; set; }
        public string position { get; set; }
        public int user_id { get; set; }
        public string status { get; set; }
    }
}
