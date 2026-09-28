using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace uBeac.Api.InputModels
{
    public class WidgetInputModel
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid DashboardId { get; set; }

        public List<WidgetInfo> Widgets { get; set; }

        public WidgetInputModel()
        {
            Widgets = new List<WidgetInfo>();
        }
    }

    public class WidgetInfo : BaseInputModelWithTeam
    {
        [StringLength(150)]
        new public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public int X { get; set; }

        [Required]
        public int Y { get; set; }

        [Required]
        public int W { get; set; }

        [Required]
        public int H { get; set; }

        [Required]
        public string Setting { get; set; }
    }
}
