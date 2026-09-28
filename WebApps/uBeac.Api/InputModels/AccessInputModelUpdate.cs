using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(AccessInputModelUpdateSchemaFilter))]
    public class AccessInputModelUpdate
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public AccessLevels Level { get; set; }
    }

    public class AccessInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Access";

            schema.Example = new AccessInputModelUpdate
            {
                Id = Guid.Empty,
                Level = AccessLevels.View
            };
        }
    }
}
