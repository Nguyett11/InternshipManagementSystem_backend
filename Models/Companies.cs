using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml;

namespace IMS.Models
{
    [Table("Companies")]
    public class Companies
    {
        [Key]
        public int company_id { get; set; }
        public string company_name { get; set; }
        public string address { get; set; }
        public string contact_person { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
    }
}
