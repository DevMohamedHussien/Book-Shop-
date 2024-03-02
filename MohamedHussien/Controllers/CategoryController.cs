using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Models;
using MohamedHussien.Utilities;

namespace MohamedHussien.Controllers
{
    [Authorize(Roles=SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWorkRepository unitOfWork;
        public CategoryController(IUnitOfWorkRepository unitOfWork)
        {
            this.unitOfWork = unitOfWork; 
        }
        public IActionResult Index()
        {
            List<Category> Data = unitOfWork.Category.GetAll().ToList();

            return View(Data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category Obj)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Create(Obj);
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            Category categoryFromDb = unitOfWork.Category.GetByCondition(x => x.Id == id);
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Update(Category obj)
        {
            Category categoryFromDb = unitOfWork.Category.GetByCondition(x => x.Id == obj.Id);
            categoryFromDb.Name = obj.Name;
            categoryFromDb.CategoryState = obj.CategoryState;
            unitOfWork.Save();
            TempData["success"] = " Category is Updated successfully  ";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Category categoryFromDb = unitOfWork.Category.GetByCondition(x => x.Id == id);

            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Delete(Category obj)
        {
            Category categoryFromDb = unitOfWork.Category.GetByCondition(x => x.Id == obj.Id);
            unitOfWork.Category.Delete(categoryFromDb);
            unitOfWork.Save();
            TempData["success"] = " Category is Deleted successfully  ";
            return RedirectToAction(nameof(Index));
        }

    }
}
