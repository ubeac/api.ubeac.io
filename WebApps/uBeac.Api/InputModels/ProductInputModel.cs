using System;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    public class ProductInputModel: BaseInputModel
    {

        [StringLength(500)]
        public string Description { get; set; }

        [Range(-100, 100, ErrorMessage = "View order is out of range!")]
        public int ViewOrder { get; set; }

        [Url(ErrorMessage = "Invalid URL!")]
        public string Url { get; set; }
        public Guid ManualFile { get; set; }
        public Guid TechnicalSpecFile { get; set; }
        public Guid Image { get; set; }

        [Required]
        public Guid ManufacturerId { get; set; }
    }
}
