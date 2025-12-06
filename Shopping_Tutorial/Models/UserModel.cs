using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
	public class UserModel
	{

		public int Id { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập user name")]
		public string Username { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập user email"), EmailAddress]
		public string Email { get; set; }
		[DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập password")]
		public string Password { get; set; }
	}
}
