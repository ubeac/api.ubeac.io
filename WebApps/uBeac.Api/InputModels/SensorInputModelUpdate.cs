using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(SensorInputModelUpdateSchemaFilter))]
    public class SensorInputModelUpdate : SensorInputModelAdd
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class SensorInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Sensor";
            schema.Example = new SensorInputModelUpdate
            {
                Id = Guid.Empty,
                Name = "Sensor name",
                TeamId = Guid.Empty,
                DeviceId = Guid.Empty,
                Uid = "string",
                Prefix = 0,
                Type = 0,
                Unit = 0,
                Persist = true,
                Description = "A brief description about sensor",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
