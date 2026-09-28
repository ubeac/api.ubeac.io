using System;

namespace uBeac.Models
{
    public class Firmware: BaseEntity
    {
        public Guid ProductId { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string TechnicalSpecFile { get; set; }
        public string Processor { get; set; }
    }
}