using System;
using System.Collections.Generic;

namespace FileRemover
{
    class File
    {
        public string Extension { get; set; }
        public long Size { get; set; }
        public Guid _id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Name { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
    }
}
