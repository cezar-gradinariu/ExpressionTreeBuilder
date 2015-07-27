namespace QueryBuilder.Entities
{
    public class SortCriteria
    {
        public SortCriteria(string propertyName, SortOrder sortOrder)
        {
            PropertyName = propertyName;
            SortOrder = sortOrder;
        }

        public string PropertyName { get; set; }
        public SortOrder SortOrder { get; set; }
    }
}