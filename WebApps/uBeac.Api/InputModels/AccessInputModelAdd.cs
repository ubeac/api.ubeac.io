using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel.DataAnnotations;
using uBeac.Models;

namespace uBeac.Api.InputModels
{
    //public class AccessInputModel : BaseInputModelWithTeam
    //{
    //    [Required]
    //    public Guid UserId { get; set; }
    //    [Required]
    //    public AccessLevels Level { get; set; }
    //}

    [SwaggerSchemaFilter(typeof(AccessInputModelAddSchemaFilter))]
    public class AccessInputModelAdd
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid TeamId { get; set; }
        
        [Required]
        public AccessLevels Level { get; set; }
    }

    public class AccessInputModelAddSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {          
            schema.Title = "Access";
            schema.Example = new AccessInputModelAdd
            {
                UserId = Guid.Empty,
                TeamId = Guid.Empty,
                Level = AccessLevels.View                
            };
        }
    }

}
