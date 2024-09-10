using System.ComponentModel.DataAnnotations;

namespace IotaWebApp.Models
{
    public class WebsiteContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ContentKey { get; set; }

        [Required]
        public string ContentValue { get; set; }
    }
}
