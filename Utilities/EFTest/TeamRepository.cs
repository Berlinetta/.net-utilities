using DALUtility.Repositories;
using EFDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTest
{
    public class TeamRepository : RepositoryBase<Team>
    {
        public TeamRepository(IRepositoryContext context) : base(context)
        {
        }
    }
}
