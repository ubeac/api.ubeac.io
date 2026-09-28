using System.ComponentModel.DataAnnotations;

namespace uBeac.Idsrv.InputModels
{
    public class ChangePasswordInputModel
    {

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string NewPassword { get; set; }

    }
}
