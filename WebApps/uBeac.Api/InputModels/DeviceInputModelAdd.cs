using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(DeviceInputModelAddSchemaFilter))]
    public class DeviceInputModelAdd
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

        [StringLength(500)]
        public string Description { get; set; }

        public Guid? FloorId { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
    }

    public class DeviceInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Device";
            schema.Example = new DeviceInputModelAdd
            {
                Name = "Device name",
                Uid = "string",
                TeamId = Guid.Empty,
                Description = "A brief description about device",
                FloorId = Guid.Empty,
                Attributes = new Dictionary<string, object>(),
                X = 0,
                Y = 0
            };
        }
    }
}
