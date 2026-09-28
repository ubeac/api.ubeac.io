using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(GatewayInputModelUpdateSchemaFilter))]
    public class GatewayInputModelUpdate : GatewayInputModelAdd
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class GatewayInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Gateway";
            schema.Example = new GatewayInputModelUpdate
            {
                Id = Guid.Empty,
                Name = "Gateway name",
                TeamId = Guid.Empty,
                FirmwareId = Guid.Empty,
                FloorId = Guid.Empty,
                Language = ProcessorLanguage.NA,
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
