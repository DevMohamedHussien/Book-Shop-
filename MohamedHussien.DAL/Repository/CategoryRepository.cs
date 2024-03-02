
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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext db;
        public CategoryRepository(ApplicationDbContext db  ) :base (db)
        {
            this.db = db; 
        }

        public void Update(Category obj)
        {
            db.Update(obj); 
        }
    }
}
