using System.ComponentModel.DataAnnotations;

namespace uBeac.Idsrv.InputModels
{
    public class ForgotPasswordInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        //[Required]
        //[Url]
        //public string CallbackUrl { get; set; }
    }
}
