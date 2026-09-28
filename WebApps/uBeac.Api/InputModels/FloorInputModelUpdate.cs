using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(FloorInputModelUpdateSchemaFilter))]
    public class FloorInputModelUpdate : FloorInputModelAdd
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class FloorInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Floor";
            schema.Example = new FloorInputModelUpdate
            {
                Id = Guid.Empty,
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
