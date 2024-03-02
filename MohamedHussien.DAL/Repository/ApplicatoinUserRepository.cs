
using E_Commerce.DAL.Repository;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Data;
using MohamedHussien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.DAL.Repository
{
    public class ApplicatoinUserRepository : Repository<ApplicationUser>, IApplicationRepository
    {
        private readonly ApplicationDbContext db;
        public ApplicatoinUserRepository(ApplicationDbContext db  ) :base (db)
        {
            this.db = db; 
        }
    }
}
