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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        ApplicationDbContext db; 
        public CompanyRepository(ApplicationDbContext db):base (db) 
        {
            this.db = db; 
        }
        public void Update(Company obj)
        {
            var objFromDb = db.Companies.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb!=null)
            {
                objFromDb.Id = obj.Id;
                objFromDb.Name = obj.Name;
                objFromDb.Address = obj.Address;
                objFromDb.City = obj.City;
                objFromDb.PostalCode = obj.PostalCode;

            }
        }
        
    }
}
