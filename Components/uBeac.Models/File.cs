using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class File : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public File()
        {
            Attributes = new Dictionary<string, object>();
        }
    }
}
