using Shopping_Tutorial.Models.Momo;
using Shopping_Tutorial.Models.Order;

namespace Shopping_Tutorial.Services.Momo
{
	public interface IMomoService
	{
		Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
	}
}
