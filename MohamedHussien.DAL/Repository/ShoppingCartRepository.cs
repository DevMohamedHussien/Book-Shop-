using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Data;
using MohamedHussien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.DAL.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartsRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db):base(db)
        {
            this._db = db;
        }
        public void Update(ShoppingCart obj)
        {
            _db.shoppingCarts.Update(obj); 
        }
    }
}
