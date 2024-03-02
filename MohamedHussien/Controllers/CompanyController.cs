using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Models;

namespace MohamedHussien.Controllers
{
    public class CompanyController : Controller
    {
        private readonly IUnitOfWorkRepository unitOfWork;

        public CompanyController(IUnitOfWorkRepository UnitOfwork)
        {
            this.unitOfWork = UnitOfwork;
        }

        public IActionResult Index()
        {
            List<Company> data = unitOfWork.Company.GetAll().ToList();
            return View(data);
        }

        [HttpGet]
        public IActionResult Upsert(int?id )
        {
            
            if (id==null||id==0 )
            {
                return View(new Company()); 
            }
            else
            {
                Company data = unitOfWork.Company.GetByCondition(x => x.Id == id);
                return View(data); 
            }
        }

        [HttpPost]
        public IActionResult Upsert( Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == null||company.Id==0)
                {
                    unitOfWork.Company.Create(company);
                }
                else
                {
                    unitOfWork.Company.Update(company);

                }
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(company); 
            }
          


        }



        #region API Calls 
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> obj = unitOfWork.Company.GetAll().ToList();
            unitOfWork.Save();
            return Json(new { data = obj });

        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyDeleted = unitOfWork.Company.GetByCondition(u => u.Id == id);
            if (CompanyDeleted == null)
            {
                return Json(new { success = false, message = " Error while deleting" });
            }
            unitOfWork.Company.Delete(CompanyDeleted);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion

    }
}
