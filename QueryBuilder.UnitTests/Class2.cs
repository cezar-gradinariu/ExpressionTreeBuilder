using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace QueryBuilder.UnitTests
{
    [TestFixture]
    public class Class2
    {
        private static readonly List<Company> List = new List<Company>
        {
            new Company("c", 3),
            new Company("c", 2),
            new Company("c", 1),
            new Company("a", 1),
            new Company("a", 2),
            new Company("a", 3),
            new Company("b", 3),
        };

        private readonly QueryObject _qObject = new QueryObject
        {
            QueryConditions = new List<QueryObject.QueryCondition>
            {
                new QueryObject.QueryCondition
                {
                    PropertyName = "Name",
                    Operation = "NotEqual",
                    Value = "b"
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
                    new SortCriteria("Name", SortOrder.Ascendant),
                    new SortCriteria("Stars", SortOrder.Ascendant)
                }
            }
        };
        public class Company
        {
            public Company(string name, int stars)
            {
                Name = name;
                Stars = stars;
            }

            public string Name { get; set; }
            public int Stars { get; set; }

            public override string ToString()
            {
                return Name + " " + Stars;
            }
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
                    List.Where(p => p.Name != "b")
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Stars)
                        .Take(1)
                        .Skip(0)
                        .ToList();
            }
            watch.Stop();
            Console.WriteLine("Direct lambda for {0} calls took {1}ms.", noOfCalls, watch.ElapsedMilliseconds);

            watch.Restart();
            var ex = _qObject.BuildExpression<Company>().Compile();
            for (var i = 0; i < noOfCalls; i++)
            {
                var s =
                    List.AsQueryable().Where(ex).AsQueryable().GetSortedPageAsList(_qObject.GridCriteria).ToList();
            }
            watch.Stop();
            Console.WriteLine("Call using cached 'where expression' for {0} calls took {1}ms.", noOfCalls,
                watch.ElapsedMilliseconds);

            watch.Restart();
            for (var i = 0; i < noOfCalls; i++)
            {
                var s =
                    List.AsQueryable()
                        .Where(_qObject.BuildExpression<Company>().Compile())
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
                    List.AsQueryable()
                        .Where(_qObject.BuildExpression<Company>())
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