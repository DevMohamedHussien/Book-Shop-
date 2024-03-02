using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Web.Helpers;

namespace MohamedHussien.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWorkRepository UnitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWorkRepository unitOfWork)
        {
            _logger = logger;
            UnitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = UnitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }
        [HttpGet]
        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = UnitOfWork.Product.GetByCondition(x => x.Id == productId),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart obj ) 
        {
          
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var UserID = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            obj.ApplicationUserId = UserID;

            ShoppingCart? shoppingCartFromDb = UnitOfWork.ShoppingCarts.GetByCondition(x => x.ApplicationUserId==UserID &&
            x.ProductId == obj.ProductId);
            if (shoppingCartFromDb!=null)
            {
                //the Cart Is Already Exist 
                shoppingCartFromDb.Count += obj.Count;
                UnitOfWork.ShoppingCarts.Update(shoppingCartFromDb);
              
            }
            else
            { 
               //There is No cart 
                UnitOfWork.ShoppingCarts.Create(obj);
            }

            UnitOfWork.Save(); 
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}