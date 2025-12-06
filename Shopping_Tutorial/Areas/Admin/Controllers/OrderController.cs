using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    //[Authorize(Roles = "Publisher,Author")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();
            //lấy shipping cost
            var Order = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();
            ViewBag.ShippingCost = Order.ShippingCost;
            ViewBag.Status = Order.Status;
            return View(DetailsOrder);
        }
        [HttpGet]
        [Route("PaymentMomoInfo")]
        public async Task<IActionResult> PaymentMomoInfo(string orderId)
        {
            var momoInfo = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderId == orderId);

            if (momoInfo == null)
            {
                return NotFound();
            }

            return View(momoInfo);
        }
        [HttpGet]
        [Route("PaymentVnpayInfo")]
        public async Task<IActionResult> PaymentVnpayInfo(string orderId)
        {
            var vnpayInfo = await _dataContext.VnInfos.FirstOrDefaultAsync(m => m.PaymentId == orderId);

            if (vnpayInfo == null)
            {
                return NotFound();
            }

            return View(vnpayInfo);
        }
        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _dataContext.Update(order);

            if (status == 0)
            {
                // lấy dữ liệu order detail dựa vào order.OrderCode
                var DetailsOrder = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                   .Where(od => od.OrderCode == order.OrderCode)
                   .Select(od => new
                   {
                       od.Quantity,
                       od.Product.Price,
                       od.Product.CapitalPrice

                   }).ToListAsync();

                // lấy data thống kê dựa vào ngày đặt hàng
                var statisticalModel = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date);

                if (statisticalModel != null)
                {
                    foreach (var orderDetail in DetailsOrder)
                    {

                        //tồn tại ngày thì cộng dồn
                        statisticalModel.Quantity += 1;
                        statisticalModel.Sold += orderDetail.Quantity;
                        statisticalModel.Revenue += orderDetail.Quantity * orderDetail.Price;
                        statisticalModel.Profit += orderDetail.Price - orderDetail.CapitalPrice;

                    }
                    _dataContext.Update(statisticalModel);
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    foreach (var orderDetail in DetailsOrder)
                    {
                        new_quantity += 1;
                        new_sold += orderDetail.Quantity;
                        new_profit += orderDetail.Price - orderDetail.CapitalPrice;

                        statisticalModel = new StatisticalModel
                        {
                            DateCreated = order.CreatedDate,
                            Quantity = new_quantity,
                            Sold = new_sold,
                            Revenue = orderDetail.Quantity * orderDetail.Price,
                            Profit = new_profit
                        };
                    }
                    _dataContext.Add(statisticalModel);

                }
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {


                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            try
            {

                //delete order
                _dataContext.Orders.Remove(order);


                await _dataContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }

    }
}
