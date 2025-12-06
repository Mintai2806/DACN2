using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Order;
using Shopping_Tutorial.Repository;

using Shopping_Tutorial.Services.Vnpay;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private static readonly HttpClient client = new HttpClient();

        private readonly IVnPayService _vnPayService;

        public CheckoutController(IVnPayService vnPayService, DataContext context)
        {
            _dataContext = context;

            _vnPayService = vnPayService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel
                {
                    OrderCode = ordercode,
                    UserName = userEmail,
                    PaymentMethod = PaymentMethod + " " + PaymentId,
                    Status = 1,
                    CreatedDate = DateTime.Now
                };

                // Nhận shipping giá từ cookie
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                if (shippingPriceCookie != null)
                {
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
                }
                orderItem.ShippingCost = shippingPrice;

                // Nhận Coupon code từ cookie
                var coupon_code = Request.Cookies["CouponTitle"];
                orderItem.CouponCode = coupon_code;

                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                // Tạo order detail
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderdetail = new OrderDetail
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductId = cart.ProductId,
                        Price = cart.Price,
                        Quantity = cart.Quantity
                    };

                    // Update product quantity
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);

                    _dataContext.Add(orderdetail);
                    _dataContext.SaveChanges();
                }

                HttpContext.Session.Remove("Cart");

                // Đã xóa gửi mail
                TempData["success"] = "Đơn hàng đã được tạo, vui lòng chờ duyệt đơn hàng nhé.";
            }

            return RedirectToAction("History", "Account");
        }




        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VnPayResponseCode == "00") // Giao dịch thành công lưu db
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now
                };
                _dataContext.Add(newVnpayInsert);
                await _dataContext.SaveChangesAsync();

                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["success"] = "Giao dịch Vnpay không thành công.";
                return RedirectToAction("Index", "Cart");
            }

            return View(response);
        }
    }
}
