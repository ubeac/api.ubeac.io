using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(TeamInputModelUpdateSchemaFilter))]
    public class TeamInputModelUpdate : TeamInputModelAdd
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class TeamInputModelUpdateSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Team";

            schema.Example = new TeamInputModelUpdate
            {
                Id = Guid.Empty,
                Name = "Team name",
                Namespace = "Team namespace",

                Address = new Address
                {
                    Address1 = "Address part1",
                    Address2 = "Address part2",
                    City = "city",
                    Province = "province",
                    PostalCode = "postal code",
                    Country = "Country"
                },
                Description = "A brief description about team",
                Attributes = new Dictionary<string, object>()
            };
        }
    }
}
