using System.Collections.Generic;

namespace QueryBuilder
{
    public class GridCriteria
    {
        public PaginationCriteria Pagination { get; set; }
        public List<SortCriteria> SortCriteria { get; set; }
    }
}