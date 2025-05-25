using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    [Table("Reports")]
    public class Reports
    {
        [Key]
        public int report_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime create_date { get; set; }
        public DateTime due_date { get; set; }
        public int lecturer_id { get; set; }
    }
}
