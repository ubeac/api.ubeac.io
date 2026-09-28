using System.ComponentModel.DataAnnotations;

namespace uBeac.Idsrv.InputModels
{
    public class ResetPasswordInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
