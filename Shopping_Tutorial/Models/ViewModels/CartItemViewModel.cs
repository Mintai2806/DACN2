

namespace Shopping_Tutorial.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; } //663190.00
        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }

    }
}
