using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Repository
{
    public interface ISelect
    {
        public List<string>? Members { get; set; }
        public List<RelationBase> Relations { get; set; }

        IRelation FindRelation(string name);
    }
}
