using Shopping_Tutorial.Models.Vnpay;

namespace Shopping_Tutorial.Services.Vnpay
{
	public interface IVnPayService
	{
		string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
		PaymentResponseModel PaymentExecute(IQueryCollection collections);
	}
}
