using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QueryBuilder
{
    internal class Program
    {
        private static void Main()
        {
            var qObject = new QueryObject
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
                        StartIndex = 0
                    },
                    SortCriteria = new List<SortCriteria>
                    {
                        new SortCriteria("Age", SortOrder.Descendant)
                    }
                }
            };

            var list = new List<Person>
            {
                new Person {Name = "B", Surname = "G", Age = 30},
                new Person {Name = "A",Surname = "G1", Age = 35},
                new Person {Name = "Ana-Maria", Surname = "Gradinariu", Age = 3},
                new Person {Name = "Maya-Ioana",Surname = "Gradinariu", Age = 1},
            };
            var watch = new Stopwatch();
            watch.Start();
                //var ex = qObject.BuildExpression<Person>().Compile();
            for (var i = 0; i < 10000; i++)
            {
                //var s = list.Where(p => p.Name.Contains("ana") && p.Surname == "Gradinariu" && p.Age <= 10);//.OrderByDescending(p=>p.Age).Take(1).Skip(0).ToList();
                //var s = list.AsQueryable().Where(qObject.BuildExpression<Person>()).GetSortedPageAsList(qObject.GridCriteria).ToList();
                var s = list.Where(qObject.BuildExpression<Person>().Compile()).AsQueryable().GetSortedPageAsList(qObject.GridCriteria).ToList();
            }

            watch.Stop();
            var ms = watch.ElapsedMilliseconds;
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Surname { get; set; }
        }
    }
}