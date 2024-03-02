using E_Commerce.DAL.Repository;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MohamedHussien.DAL.Repository.IRepository;
using MohamedHussien.Models;
using MohamedHussien.Models.ViewModels;
using MohamedHussien.Utilities;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace MohamedHussien.Controllers
{
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWorkRepository _UnitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWorkRepository UnitOfWork)
        {
			this._UnitOfWork = UnitOfWork; 
        }

        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId )
        {
             OrderVM = new()
            {
                orderHeader = _UnitOfWork.OrderHeader.GetByCondition(u => u.Id == orderId, includeProperties: "applicationUser"),
                orderDetail = _UnitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product").ToList()

            }; 
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetailsl( )
        {
            var orderHeaderFromDb = _UnitOfWork.OrderHeader.GetByCondition(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.orderHeader.Name; 
            orderHeaderFromDb.Phone = OrderVM.orderHeader.Phone; 
            orderHeaderFromDb.StreetAdress = OrderVM.orderHeader.StreetAdress; 
            orderHeaderFromDb.City = OrderVM.orderHeader.City; 
            orderHeaderFromDb.State = OrderVM.orderHeader.State; 
            orderHeaderFromDb.PostalCode = OrderVM.orderHeader.PostalCode;
            _UnitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _UnitOfWork.Save();
            TempData["Success"] = "Order Details is updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id }); 
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StrtProcessing()
        {
            _UnitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusInProcess);
            _UnitOfWork.Save();
            TempData["Success"] = "Order Details is updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id }); 

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _UnitOfWork.OrderHeader.GetByCondition(u => u.Id == OrderVM.orderHeader.Id);
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus==SD.PaymentStatusDelayedApproved)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30); 
            }
            _UnitOfWork.OrderHeader.Update(orderHeader);
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Details),new { orderId = OrderVM.orderHeader.Id }); 
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _UnitOfWork.OrderHeader.GetByCondition(u => u.Id == OrderVM.orderHeader.Id);
            if (orderHeader.PaymentStatus==SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusReFunded); 
            }
            else
            {
                _UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusReFunded); 
            }
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }
        [HttpPost]
        [ActionName("Details")]  
        public IActionResult Details_Pay_Now()
        {
            OrderVM.orderHeader = _UnitOfWork.OrderHeader
                .GetByCondition(u => u.Id == OrderVM.orderHeader.Id,includeProperties: "applicationUser");
            OrderVM.orderDetail = _UnitOfWork.OrderDetails
                .GetAll(u => u.OrderHeaderId == OrderVM.orderHeader.Id,includeProperties: "Product").ToList();
            var domain = "https://localhost:7066/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"Order/OrderConfirmation?orderHeaderId={OrderVM.orderHeader.Id}",
                CancelUrl = domain + $"Order/Details?orderId={OrderVM.orderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.orderDetail)
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
            Session session = service.Create(options);
            _UnitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _UnitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }
        public IActionResult OrderConfirmation(int orderHeaderId )
        {
            OrderHeader orderHeader = _UnitOfWork.OrderHeader.GetByCondition(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedApproved)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _UnitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _UnitOfWork.OrderHeader.UpdateStatus(orderHeaderId, SD.StatusApproved, SD.PaymentStatusApproved);
                    _UnitOfWork.Save();
                }
            }
            return View(orderHeaderId);

        }
        #region API Calls 
        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> obj; 
            if (User.IsInRole(SD.Role_Admin)||User.IsInRole(SD.Role_Employee))
            {
                obj= _UnitOfWork.OrderHeader.GetAll(includeProperties: "applicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;

                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                obj = _UnitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "applicationUser"); 
                    
            }
            switch (status)
            {
                case "pending":
                    obj = obj.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedApproved); 
                          break;
                case "inProcess":
                    obj = obj.Where(u => u.OrderStatus == SD.StatusInProcess); 
                          break;
                case "completed":
                    obj = obj.Where(u => u.OrderStatus == SD.StatusShipped);
                          break;
                case "approved":
                    obj = obj.Where(u => u.OrderStatus == SD.StatusApproved);
                         break;
                default:
                         break;
            }
            return Json(new { data = obj });
		}
		#endregion
	}
}