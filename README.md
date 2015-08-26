# ExpressionTreeBuilder
Simple Expression tree builder from basic query object

Given an object as input I want to transform it into an expression tree. That can be fed to an IQueryable<T> and both fitering 
sorting should be handled automatically.

1. Given this object:
QueryObject _qObject = new QueryObject
        {
            QueryConditions = new List<QueryObject.QueryCondition>
            {
                new QueryObject.QueryCondition
                {
                    PropertyName = "Name",
                    Operation = "Contains",
                    Value = "na"
                },
                new QueryObject.QueryCondition
                {
                    PropertyName = "Surname",
                    Operation = "Equal",
                    Value = "Gradinariu"
                },
                new QueryObject.QueryCondition
                {
                    PropertyName = "Age",
                    Operation = "LessThanOrEqual",
                    Value = 10
                }
            },
            GridCriteria = new GridCriteria
            {
                Pagination = new PaginationCriteria
                {
                    PageSize = 1,
                    StartIndex = 1
                },
                SortCriteria = new List<SortCriteria>
                {
                    new SortCriteria("Age", SortOrder.Descendant),
                    new SortCriteria("Name", SortOrder.Ascendant),
                    new SortCriteria("Surname", SortOrder.Ascendant)
                }
            }
        };
        
2. This list
IQueryable<Person> _list = new List<Person>
        {
            new Person {Name = "B", Surname = "G", Age = 30},
            new Person {Name = "A", Surname = "G1", Age = 35},
            new Person {Name = "Ana-Maria", Surname = "Gradinariu", Age = 3},
            new Person {Name = "Maya-Ioana", Surname = "Gradinariu", Age = 1},
            new Person {Name = "B", Surname = "G", Age = 30},
            new Person {Name = "A", Surname = "G1", Age = 35},
            new Person {Name = "Ana-Marid", Surname = "Gradinariu", Age = 3},
            new Person {Name = "Maya-Ioana", Surname = "Gradinariu", Age = 1},
        }.AsQueryable();
        
3. We want to be able to do this call:
_list.GetQuery(_qObject).ToList();

4. Results:
Compared to a direct lambda expression this is extremly slow, at around 500 times slower. For 1000 calls it takes on my machine
3200ms. It is slow, but it is also extremely dynamic, so in case you have a UI with a lot of possbile inputs, this solution is
acceptable as it reduces the amount of coding and everything happens in the same place - the transformation classes.

It also exposes at the moment only a subset of possbile operations, like "Contains", "Equal", "GreaterThan", etc.


