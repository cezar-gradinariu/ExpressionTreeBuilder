using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace QueryBuilder.UnitTests
{
    [TestFixture]
    public class Class1
    {
        private readonly QueryObject _qObject = new QueryObject
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

        private readonly List<Person> _list = new List<Person>
        {
            new Person {Name = "B", Surname = "G", Age = 30},
            new Person {Name = "A", Surname = "G1", Age = 35},
            new Person {Name = "Ana-Maria", Surname = "Gradinariu", Age = 3},
            new Person {Name = "Maya-Ioana", Surname = "Gradinariu", Age = 1},
            new Person {Name = "B", Surname = "G", Age = 30},
            new Person {Name = "A", Surname = "G1", Age = 35},
            new Person {Name = "Ana-Maria", Surname = "Gradinariu", Age = 3},
            new Person {Name = "Maya-Ioana", Surname = "Gradinariu", Age = 1},
        };


        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Surname { get; set; }
        }

        [Test]
        public void PerformTimeTests()
        {
            const int noOfCalls = 1000;
            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < noOfCalls; i++)
            {
                var s =
                    _list.Where(p => p.Name.Contains("na") && p.Surname == "Gradinariu" && p.Age <= 10)
                        .OrderByDescending(p => p.Age)
                        .ThenBy(p => p.Name)
                        .ThenBy(p => p.Surname)
                        .Take(1)
                        .Skip(0)
                        .ToList();
            }
            watch.Stop();
            Console.WriteLine("Direct lambda for {0} calls took {1}ms.", noOfCalls, watch.ElapsedMilliseconds);

            watch.Restart();
            var ex = _qObject.BuildExpression<Person>().Compile();
            for (var i = 0; i < noOfCalls; i++)
            {
                var s =
                    _list.AsQueryable().Where(ex).AsQueryable().GetSortedPageAsList(_qObject.GridCriteria).ToList();
            }
            watch.Stop();
            Console.WriteLine("Call using cached 'where expression' for {0} calls took {1}ms.", noOfCalls,
                watch.ElapsedMilliseconds);

            watch.Restart();
            for (var i = 0; i < noOfCalls; i++)
            {
                var s =
                    _list.AsQueryable()
                        .Where(_qObject.BuildExpression<Person>().Compile())
                        .AsQueryable()
                        .GetSortedPageAsList(_qObject.GridCriteria)
                        .ToList();
            }
            watch.Stop();
            Console.WriteLine("Call with 'where expression' compiled explicitly for {0} calls took {1}ms.", noOfCalls,
                watch.ElapsedMilliseconds);


            watch.Restart();
            for (var i = 0; i < noOfCalls; i++)
            {
                var s =
                    _list.AsQueryable()
                        .Where(_qObject.BuildExpression<Person>())
                        .AsQueryable()
                        .GetSortedPageAsList(_qObject.GridCriteria)
                        .ToList();
            }
            watch.Stop();
            Console.WriteLine("Call with 'where expression' not compiled for {0} calls took {1}ms.", noOfCalls,
                watch.ElapsedMilliseconds);
        }
    }
}