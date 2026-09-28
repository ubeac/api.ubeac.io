using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(GatewayInputModelAddSchemaFilter))]
    public class GatewayInputModelAdd
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        [Required]
        public Guid FirmwareId { get; set; }

        public decimal X { get; set; }
        public decimal Y { get; set; }
        public Guid? FloorId { get; set; }
        public ProcessorLanguage Language { get; set; }
        public string Processor { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9]*$")]
        public string Url { get; set; }

        [Required]
        public GatewaySecurity Security { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class GatewayInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {            
            schema.Title = "Gateway";
            schema.Example = new GatewayInputModelAdd
            {
                Name = "Gateway name",
                TeamId = Guid.Empty,
                FirmwareId = Guid.Empty,
                FloorId = Guid.Empty,
                Language= ProcessorLanguage.NA,
                Processor = "string",
                Security = new GatewaySecurity(),
                Url = "string", 
                X = 0,
                Y = 0,
                Description = "A brief description about gateway",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
