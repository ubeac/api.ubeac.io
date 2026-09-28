using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(TeamInputModelGetTokenSchemaFilter))]
    public class TeamInputModelGetToken
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public AccessLevels AccessLevel { get; set; }
    }

    public class TeamInputModelGetTokenSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Access Token";

            schema.Example = new TeamInputModelGetToken
            {
                TeamId = Guid.Empty,
                AccessLevel = AccessLevels.View
            };
        }
    }
}
