using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(BuildingInputModelUpdateSchemaFilter))]
    public class BuildingInputModelUpdate : BuildingInputModelAdd
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class BuildingInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Building";
            schema.Example = new BuildingInputModelUpdate
            {
                Id = Guid.Empty,
                Name = "Building name",
                TeamId = Guid.Empty,
                Address = new Address
                {
                    Address1 = "Address part1",
                    Address2 = "Address part2",
                    City = "city",
                    Province = "province",
                    PostalCode = "postal code",
                    Country = "Country"
                },
                Latitude = 000000,
                Longitude = 000000,
                Description = "A brief description about building",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
