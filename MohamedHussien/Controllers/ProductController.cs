using Microsoft.AspNetCore.Mvc;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Models.ViewModels;
using MohamedHussien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using MohamedHussien.Utilities;

namespace MohamedHussien.Controllers.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWorkRepository unitOfWork;
        private readonly IWebHostEnvironment webHost;
        public ProductController(IUnitOfWorkRepository unitOfWork, IWebHostEnvironment webHost)
        {
            this.unitOfWork = unitOfWork;
            this.webHost = webHost;
        }
        [HttpGet]
        public IActionResult Index()
        {
            List<Product> data = unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(data);
        }
        public IActionResult UpSert(int? id)
        {
            

            ProductVM productVM = new()
            {
                CategoryList = unitOfWork.Category.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = unitOfWork.Product.GetByCondition(x => x.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult UpSert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwrootPath = webHost.WebRootPath;
                if (file != null)

                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string ImagePath = Path.Combine(wwwrootPath, @"Images\Product");
                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwrootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(ImagePath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\Images\Product\" + fileName;
                }
                if (obj.Product.Id == null || obj.Product.Id == 0)
                {
                    unitOfWork.Product.Create(obj.Product);
                }
                else
                {
                    unitOfWork.Product.Update(obj.Product);
                }
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                obj.CategoryList = unitOfWork.Category.GetAll()
                    .Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });
                return View(obj);
            }

        }



        #region API Calls 
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> obj = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data=obj });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productForDeleted = unitOfWork.Product.GetByCondition(u => u.Id == id);
            if (productForDeleted == null)
            {
                return Json(new { success = false, message = " Error while deleting" });
            }
            var oldImagePath = Path.Combine(webHost.WebRootPath
                , productForDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            unitOfWork.Product.Delete(productForDeleted);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
