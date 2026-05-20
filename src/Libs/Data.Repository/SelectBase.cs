using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Repository
{
    public class SelectBase : ISelect
    {
        public List<string>? Members { get; set; }
        public List<RelationBase> Relations { get; set; }

        public SelectBase()
        {
            Members = new List<string>();
            Relations = new List<RelationBase>();
        }

        public IRelation FindRelation(string name)
        {
            return Relations.FirstOrDefault(_ => _.Name == name);
        }
    }
}
