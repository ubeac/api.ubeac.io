using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(FloorInputModelAddSchemaFilter))]
    public class FloorInputModelAdd
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        [Required]
        public Guid BuildingId { get; set; }
        public Guid PlanFileId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class FloorInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Floor";
            schema.Example = new FloorInputModelAdd
            {
                Name = "Floor name",
                TeamId = Guid.Empty,
                BuildingId = Guid.Empty,
                PlanFileId = Guid.Empty,
                Description = "A brief description about floor",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
