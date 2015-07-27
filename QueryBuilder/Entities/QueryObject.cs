using System.Collections.Generic;

namespace QueryBuilder.Entities
{
    public class QueryObject
    {
        public List<QueryCondition> QueryConditions { get; set; }
        public GridCriteria GridCriteria { get; set; }

        public class QueryCondition
        {
            public string PropertyName { get; set; }
            public object Value { get; set; }
            public string Operation { get; set; }
        }
    }
}