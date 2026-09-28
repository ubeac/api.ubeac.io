using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    public abstract class BaseInputModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        public Dictionary<string, object> Attributes { get; set; }
    }

    public abstract class BaseInputModelWithTeam: BaseInputModel
    {
        [Required]
        public Guid TeamId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

}
