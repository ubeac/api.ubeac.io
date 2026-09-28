using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(SensorInputModelAddSchemaFilter))]
    public class SensorInputModelAdd
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        [Required]
        [StringLength(50)]
        public string Uid { get; set; }

        [Required]
        public int Type { get; set; }

        public int Unit { get; set; }
        public int Prefix { get; set; }

        [Required]
        public Guid DeviceId { get; set; }

        public bool Persist { get; set; } = true;

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class SensorInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Sensor";
            schema.Example = new SensorInputModelAdd
            {
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
