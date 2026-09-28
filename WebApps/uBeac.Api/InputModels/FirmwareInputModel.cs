using System;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    public class FirmwareInputModel : BaseInputModel
    {

        [Required]
        public Guid ProductId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        public Guid TechnicalSpecFile { get; set; }
        public string Processor { get; set; }
    }
}
