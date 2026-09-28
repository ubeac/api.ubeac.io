using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(DashboardInputModelUpdateSchemaFilter))]
    public class DashboardInputModelUpdate : DashboardInputModelAdd
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class DashboardInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Dashboard";
            schema.Example = new DashboardInputModelUpdate
            {
                Id = Guid.Empty,
                Name = "Dashboard name",
                TeamId = Guid.Empty,
                ViewOrder = 0,
                Description = "A brief description about dashboard",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
