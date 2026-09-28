using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    [SwaggerSchemaFilter(typeof(TeamInputModelRemoveTokenSchemaFilter))]
    public class TeamInputModelRemoveToken
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public string Token { get; set; }
    }

    public class TeamInputModelRemoveTokenSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Title = "Access Token";

            schema.Example = new TeamInputModelRemoveToken
            {
                TeamId = Guid.Empty,
                Token = "access token string"
            };
        }
    }
}
