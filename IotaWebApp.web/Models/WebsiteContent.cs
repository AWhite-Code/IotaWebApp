using System.ComponentModel.DataAnnotations;

namespace IotaWebApp.Models
{
    public class WebsiteContent
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Content Key is required.")]
        [StringLength(50)]
        public string ContentKey { get; set; }

        [StringLength(5000)] // Adjust the length as needed
        public string ContentValue { get; set; }

        [Required(ErrorMessage = "Content Type is required.")]
        [StringLength(20)]
        public string ContentType { get; set; }
    }
}
