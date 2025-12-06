using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
	public class BrandModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được bỏ trống tên thương hiệu")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được bỏ trống mô tả")]
		public string Description { get; set; }
		public string Slug { get; set; }
		public int? Status { get; set; }

	}
}
