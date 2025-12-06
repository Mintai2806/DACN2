using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Order;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Shopping_Tutorial.Controllers
{


	public class PaymentController : Controller
	{
		private IMomoService _momoService;
		private readonly IVnPayService _vnPayService;
		public PaymentController(IMomoService momoService, IVnPayService vnPayService)
		{
			_momoService = momoService;
			_vnPayService = vnPayService;
		}

		[HttpPost]
		[Route("CreatePaymentUrl")]
		public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			return Redirect(response.PayUrl);
		}

		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

			return Redirect(url);
		}



	}
}
