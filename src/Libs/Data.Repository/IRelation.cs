using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Repository
{
    public interface IRelation
    {
        public string? Name { get; set; }
        public SelectBase? Select { get; set; }
    }
}
