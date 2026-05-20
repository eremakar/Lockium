using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Repository
{
    public class RelationBase : IRelation
    {
        public string? Name { get; set; }
        public SelectBase? Select { get; set; }
    }
}
