using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext _context)
		{
			_dataContext = _context;
		}
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			// Nhận shipping giá từ cookie
			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
			decimal shippingPrice = 0;

			//Nhận Coupon code từ cookie
			var coupon_code = Request.Cookies["CouponTitle"];

			if (shippingPriceCookie != null)
			{
				var shippingPriceJson = shippingPriceCookie;
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
			}

			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
				ShippingCost = shippingPrice,
				CouponCode = coupon_code

			};
			return View(cartVM);
		}

		//public IActionResult Checkout()
		//{
		//	return View();
		//}

		public async Task<IActionResult> Add(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItems == null)
			{
				cart.Add(new CartItemModel(product));
			}
			else
			{
				cartItems.Quantity += 1;
			}

			HttpContext.Session.SetJson("Cart", cart);

			TempData["success"] = "Add Product to cart Sucessfully! ";
			return Redirect(Request.Headers["Referer"].ToString());
		}
		public async Task<IActionResult> Decrease(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if (cartItem.Quantity > 1)
			{
				--cartItem.Quantity;
			}
			else
			{
				cart.RemoveAll(p => p.ProductId == Id);
			}
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
			}

			TempData["success"] = "Decrease Product to cart Sucessfully! ";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Increase(int Id)
		{
			ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
			{
				++cartItem.Quantity;
				TempData["success"] = "Increase Product to cart Sucessfully! ";
			}
			else
			{
				cartItem.Quantity = product.Quantity;
				TempData["success"] = "Maximum Product Quantity to cart Sucessfully! ";
			}
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
			}

			TempData["success"] = "Increase Product to cart Sucessfully! ";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Remove(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			cart.RemoveAll(p => p.ProductId == Id);
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
			}

			TempData["success"] = "Remove Product to cart Sucessfully! ";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Clear()
		{
			HttpContext.Session.Remove("Cart");

			TempData["success"] = "Clear all Product to cart Sucessfully! ";
			return RedirectToAction("Index");
		}
		//tính phí shipping
		[HttpPost]
		[Route("Cart/GetShipping")]
		public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
		{

			var existingShipping = await _dataContext.Shippings
				.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

			decimal shippingPrice = 0; // Set mặc định giá tiền

			if (existingShipping != null)
			{
				shippingPrice = existingShipping.Price;
			}
			else
			{
				//Set mặc định giá tiền nếu ko tìm thấy
				shippingPrice = 50000;
			}
			var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
			try
			{
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true // using HTTPS
				};

				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
			}
			catch (Exception ex)
			{
				//
				Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
			}

			return Json(new { shippingPrice });
		}
		[HttpGet]
		[Route("Cart/DeleteShipping")]
		public IActionResult DeleteShipping()
		{
			Response.Cookies.Delete("ShippingPrice");
			return RedirectToAction("Index", "Cart");
		}
		//Hàm GetCoupon Code
		[HttpPost]
		[Route("Cart/GetCoupon")]
		public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
		{
			var validCoupon = await _dataContext.Coupons
				.FirstOrDefaultAsync(x => x.Name == coupon_value);

			string couponTitle = validCoupon.Name + " | " + validCoupon?.Description;

			if (couponTitle != null)
			{
				TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
				int daysRemaining = remainingTime.Days;

				if (daysRemaining >= 0)
				{
					try
					{
						var cookieOptions = new CookieOptions
						{
							HttpOnly = true,
							Expires = DateTimeOffset.UtcNow.AddMinutes(30),
							Secure = true,
							SameSite = SameSiteMode.Strict // Kiểm tra tính tương thích trình duyệt
						};

						Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
						return Ok(new { success = true, message = "Coupon applied successfully" });
					}
					catch (Exception ex)
					{
						//trả về lỗi 
						Console.WriteLine($"Error adding apply coupon cookie: {ex.Message}");
						return Ok(new { success = false, message = "Coupon applied failed" });
					}
				}
				else
				{

					return Ok(new { success = false, message = "Coupon has expired" });
				}

			}
			else
			{
				return Ok(new { success = false, message = "Coupon not existed" });
			}

			return Json(new { CouponTitle = couponTitle });
		}

	}
}
