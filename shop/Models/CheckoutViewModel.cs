using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace shop.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Họ tên bắt buộc nhập")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại bắt buộc nhập")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ bắt buộc nhập")]
        public string Address { get; set; } = string.Empty;

        // Dùng để hiển thị giỏ hàng trên trang thanh toán
        public List<CartItem> Items { get; set; } = new();

        public int Total { get; set; }
    }
}
