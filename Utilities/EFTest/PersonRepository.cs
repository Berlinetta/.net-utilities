using DAL.Fundamentals.Repositories;
using EFDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTest
{
    public class PersonRepository : RepositoryBase<Person>
    {
        public PersonRepository(IRepositoryContext context) : base(context)
        { 
        }
    }
}
