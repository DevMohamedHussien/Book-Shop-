using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Data;
using MohamedHussien.Models;
using Microsoft.EntityFrameworkCore;

namespace MohamedHussien.DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbset; 
        public Repository(ApplicationDbContext db)
        {
            this._db = db;
            this.dbset = db.Set<T>();
            _db.Products.Include(u=>u.Category).Include(u=>u.CategoryId); 
        }
        public void Create(T entity)
        {
            dbset.Add(entity); 
        }

        public void Delete(T entity)
        {
            dbset.Remove(entity); 
        }

        public void DeleteRange(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity); 
        }

        public IEnumerable<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>>? filter ,string? includeProperties= null)
        {
            IQueryable<T> query = dbset;
            if (filter!=null)
            {
                query = query.Where(filter);
            }
            
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var item in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item); 
                }
            }
            
            return query.ToList(); 
        }

        public T GetByCondition(System.Linq.Expressions.Expression<Func<T, bool>> filter , string? includeProperties = null)
        {
            IQueryable<T> query = dbset;
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var item in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.FirstOrDefault(); 
        }
    }
}
