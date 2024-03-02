using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.DAL.Repository.IRepository
{
    public  interface IUnitOfWorkRepository
    {
        ICategoryRepository Category { get;  }
        IProductRepository Product { get; }
        ICompanyRepository Company { get;  }
        IShoppingCartsRepository ShoppingCarts { get; }
        IApplicationRepository ApplicationUser { get;  }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailsRepository OrderDetails { get;  }
        void Save();
    }
}
