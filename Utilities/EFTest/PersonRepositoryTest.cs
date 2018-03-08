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
            var newId = Guid.NewGuid();
            pr.Add(new Person() { Id = newId, Name = "toby1", Address = "shenzhen" });

            using (var db = new MyDBContext())
            {
                var query = from p in db.Persons
                            where p.Id == newId
                            select p;
                var count = query.Count();

                Assert.IsTrue(count == 1);
                Assert.IsTrue(query.First().Id == newId);
            }
            var es = new ExpressionSpecification<Person>(p => p.Id.Equals(newId));
            pr.RemoveAll(es);
        }

        [TestMethod]
        [Description("Function 'Get' test with param filter condition")]
        public void GetTest1()
        {
            var newId = Guid.NewGuid();
            pr.Add(new Person() { Id = newId, Name = "toby1", Address = "shenzhen" });

            var con = new FilterCondition();
            var fs = new FilterSpecification<Person>(new List<FilterCondition>() {
                new FilterCondition() {PropertyName = "Id", FilterOperate = FilterOperate.Equals, PropertyValues = new List<object> { newId } }
            });
            var person = pr.Get(fs);
            Assert.IsTrue(person.Id == newId);
            pr.RemoveAll(fs);
        }

        [TestMethod]
        [Description("Function 'Get' test with param filter condition")]
        public void GetTest2()
        {
            var newId = Guid.NewGuid();
            pr.Add(new Person() { Id = newId, Name = "toby1", Address = "shenzhen" });
            var es = new ExpressionSpecification<Person>(p => p.Id.Equals(newId));
            var person = pr.Get(es);
            Assert.IsTrue(person.Id == newId);
            pr.RemoveAll(es);
        }
    }
}
