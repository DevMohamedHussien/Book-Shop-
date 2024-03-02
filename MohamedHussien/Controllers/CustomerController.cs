using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Models;
using MohamedHussien.Models.ViewModels;
using MohamedHussien.Utilities;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace MohamedHussien.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;
        [BindProperty]
        public ShoppingVM shoppingVM { get; set; }
        public CustomerController(IUnitOfWorkRepository unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
           
            shoppingVM = new()
            {
                shoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == UserId,
                includeProperties: "Product"),
                OrderHeader=new ()

            };
            foreach (var item in shoppingVM.shoppingCartList)
            {
                item.Price = GetPriceBaseOnQuantity(item);
                shoppingVM.OrderHeader.TaotalOrder += (item.Price * item.Count); 
            }

            return View(shoppingVM);
        }

        public IActionResult Summery()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingVM.shoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == UserId,
                includeProperties: "Product");

            shoppingVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingVM.OrderHeader.ApplicationUserId= UserId; 
          
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetByCondition(u => u.Id == UserId);

            foreach (var item in shoppingVM.shoppingCartList) 
            {
                item.Price = GetPriceBaseOnQuantity(item);
                shoppingVM.OrderHeader.TaotalOrder += (item.Price * item.Count);
            }
            if (applicationUser.CompanyId.GetValueOrDefault()==0)
            {
                shoppingVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingVM.OrderHeader.OrderStatus = SD.StatusApproved; 
            }
            else
            {
				shoppingVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedApproved;
				shoppingVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
            _unitOfWork.OrderHeader.Create(shoppingVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var item in shoppingVM.shoppingCartList)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetails.Create(orderDetails);
                _unitOfWork.Save(); 
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                var domain = "https://localhost:7066/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/OrderConfirmation?id={shoppingVM.OrderHeader.Id}",
                    CancelUrl = domain+ "Customer/Index",
					LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
					Mode = "payment",
				};

                foreach (var item in shoppingVM.shoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {

                        PriceData = new SessionLineItemPriceDataOptions
                        {

                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem); 
                }

				var service = new Stripe.Checkout.SessionService();
		        Session session= service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(shoppingVM.OrderHeader.Id, session.Id, session.PaymentIntentId );
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); 
			}
            return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingVM.OrderHeader.Id }); 
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetByCondition(u => u.Id == id, includeProperties: "applicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedApproved)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower()=="paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save(); 
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCarts
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCarts.DeleteRange(shoppingCarts);
            _unitOfWork.Save(); 
            return View(id); 
        
        }
        [HttpGet]
        [ActionName("Summery")]
		public IActionResult SummeryPOST( ShoppingVM obj )
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			shoppingVM = new()
			{
				shoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == UserId,
				includeProperties: "Product"),
				OrderHeader = new()

			};
			shoppingVM.OrderHeader.applicationUser = _unitOfWork.ApplicationUser.GetByCondition(u => u.Id == UserId);

			shoppingVM.OrderHeader.Name = shoppingVM.OrderHeader.applicationUser.Name;
			shoppingVM.OrderHeader.StreetAdress = shoppingVM.OrderHeader.applicationUser.Address;
			shoppingVM.OrderHeader.City = shoppingVM.OrderHeader.applicationUser.City;
			shoppingVM.OrderHeader.PostalCode = shoppingVM.OrderHeader.applicationUser.PostalCode;
			shoppingVM.OrderHeader.State = shoppingVM.OrderHeader.applicationUser.City;

			foreach (var item in shoppingVM.shoppingCartList)
			{
				item.Price = GetPriceBaseOnQuantity(item);
				shoppingVM.OrderHeader.TaotalOrder += (item.Price * item.Count);
			}

			return View(shoppingVM);
		}

		public IActionResult Increment(int cartId)
        {
            var CartFromDb = _unitOfWork.ShoppingCarts.GetByCondition(x => x.Id == cartId);
            CartFromDb.Count += 1;
            _unitOfWork.ShoppingCarts.Update(CartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index)); 

        }

        public IActionResult Decrement(int cartId)
        {
            
            var CartFromDb = _unitOfWork.ShoppingCarts.GetByCondition(x => x.Id == cartId);
            if (CartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCarts.Delete(CartFromDb);
            }
            else
            {
                CartFromDb.Count -= 1;
                _unitOfWork.ShoppingCarts.Update(CartFromDb);
            }
           
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Remove(int cartId)
        {
            var CartFromDb = _unitOfWork.ShoppingCarts.GetByCondition(x => x.Id == cartId);
            _unitOfWork.ShoppingCarts.Delete(CartFromDb); 
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceBaseOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count<=50)
            {
                return shoppingCart.Product.Price; 
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }

            }
        }
    }
}