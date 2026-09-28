using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(DashboardInputModelAddSchemaFilter))]
    public class DashboardInputModelAdd
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        [Required]
        public double ViewOrder { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class DashboardInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Dashboard";
            schema.Example = new DashboardInputModelAdd
            {
                Name = "Dashboard name",
                TeamId = Guid.Empty,
                ViewOrder = 0,
                Description = "A brief description about Dashboard",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
