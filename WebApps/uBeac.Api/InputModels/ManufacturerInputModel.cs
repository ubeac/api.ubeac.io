using System;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    public class ManufacturerInputModel : BaseInputModel
    {
        [StringLength(500)]
        public string Description { get; set; }

        [Url(ErrorMessage = "Invalid URL!")]
        [Required]
        public string Website { get; set; }
        public Guid Logo { get; set; }
    }
}
