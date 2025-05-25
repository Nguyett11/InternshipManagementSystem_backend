using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography.Xml;

namespace IMS.Models
{
    [Table("Mentors")]
    public class Mentors
    {
        [Key]
        public int mentor_id { get; set; }
        public string mentor_name { get; set; }
        public int company_id { get; set; }
        public string position { get; set; }
        public int user_id { get; set; }
    }
}
