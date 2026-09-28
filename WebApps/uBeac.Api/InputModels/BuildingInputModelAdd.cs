using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(BuildingInputModelAddSchemaFilter))]
    public class BuildingInputModelAdd
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public Guid TeamId { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public Address Address { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class BuildingInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Building";
            schema.Example = new BuildingInputModelAdd
            {
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
