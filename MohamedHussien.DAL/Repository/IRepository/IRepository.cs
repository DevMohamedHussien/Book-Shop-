using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.DAL.Repository.IRepository
{
    public interface IRepository<T> where T:class  
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null);
        T GetByCondition(Expression<Func<T, bool>> filter, string? includeProperties = null);
        void Create(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entity );
        

    }
}
