using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(DeviceInputModelUpdateSchemaFilter))]
    public class DeviceInputModelUpdate
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public Guid? FloorId { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
    }

    public class DeviceInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Device";
            schema.Example = new DeviceInputModelUpdate
            {
                Id = Guid.Empty,
                Name = "Device name",
                TeamId = Guid.Empty,
                FloorId = Guid.Empty,
                Description = "A brief description about device",
                Attributes = new Dictionary<string, object>(),
                X = 0,
                Y = 0
            };
        }
    }
}
