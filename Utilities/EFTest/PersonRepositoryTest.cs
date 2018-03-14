using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EFDemo;
using DAL.Fundamentals.Repositories;
using DAL.Fundamentals.Specifications;
using DAL.Fundamentals.Data;
using System.Collections.Generic;

namespace EFTest
{
    [TestClass]
    public class PersonRepositoryTest
    {
        private PersonRepository pr = new PersonRepository(new RepositoryContext<MyDBContext>(new MyDBContext()));
        private TeamRepository tr = new TeamRepository(new RepositoryContext<MyDBContext>(new MyDBContext()));

        [TestMethod]
        public void MyDBContextTest()
        {
            using (var db = new MyDBContext())
            {
            }
        }

        [TestMethod]
        public void CountTest()
        {
            using (var db = new MyDBContext())
            {
                var query = from p in db.Persons
                            select p;
                Assert.IsTrue(pr.Count(new AnySpecification<Person>()) == query.Count());
            }
        }

        [TestMethod]
        public void AddTest()
        {
            var newTeamId = Guid.NewGuid();
            tr.Add(new Team() { Id = newTeamId, Name = "MA" });
            var newPersonId = Guid.NewGuid();
            pr.Add(new Person() { Id = newPersonId, Name = "toby1", Address = "shenzhen", Age = "11", TeamId = newTeamId });

            using (var db = new MyDBContext())
            {
                var query = from p in db.Persons
                            where p.Id == newPersonId
                            select p;
                var count = query.Count();

                Assert.IsTrue(count == 1);
                Assert.IsTrue(query.First().Id == newPersonId);
            }
            var es = new ExpressionSpecification<Person>(p => p.Id.Equals(newPersonId));
            pr.RemoveAll(es);
            var ts = new ExpressionSpecification<Team>(t => t.Id.Equals(newTeamId));
            tr.RemoveAll(ts);
        }

        [TestMethod]
        [Description("Function 'Get' test with param filter condition")]
        public void GetTest1()
        {
            var newTeamId = Guid.NewGuid();
            tr.Add(new Team() { Id = newTeamId, Name = "MA" });
            var newPersonId = Guid.NewGuid();
            pr.Add(new Person() { Id = newPersonId, Name = "toby1", Address = "shenzhen", Age = "11", TeamId = newTeamId });

            var con = new FilterCondition();
            var fs = new FilterSpecification<Person>(new List<FilterCondition>() {
                new FilterCondition() {PropertyName = "Id", FilterOperate = FilterOperate.Equals, PropertyValues = new List<object> { newPersonId } }
            });
            var person = pr.Get(fs);
            Assert.IsTrue(person.Id == newPersonId);
            pr.RemoveAll(fs);
            var ts = new ExpressionSpecification<Team>(t => t.Id.Equals(newTeamId));
            tr.RemoveAll(ts);
        }
    }
}
