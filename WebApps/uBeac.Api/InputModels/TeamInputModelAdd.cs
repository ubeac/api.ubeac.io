using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(TeamInputModelAddSchemaFilter))]
    public class TeamInputModelAdd
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(50, MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9]*$")]
        public string Namespace { get; set; }

        public Dictionary<string, object> Attributes { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }

        public Address Address { get; set; }
    }

    public class TeamInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Team";

            schema.Example = new TeamInputModelAdd
            {
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
