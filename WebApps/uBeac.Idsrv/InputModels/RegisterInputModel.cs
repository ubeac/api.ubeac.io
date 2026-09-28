using System.ComponentModel.DataAnnotations;

namespace uBeac.Idsrv.InputModels
{
    public class RegisterInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Password { get; set; }

        [Required]
        public int TimeZoneOffset { get; set; }

        [Required]
        public string TimeZone { get; set; }
    }
}
