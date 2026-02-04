using System.ComponentModel.DataAnnotations;

namespace shop.Models.ViewModels
{
    public class CustomerInfoViewModel
    {
        public int MaKh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string Ten { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? DienThoai { get; set; }

        [EmailAddress]
        [StringLength(50)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        // Nếu muốn cho đổi mật khẩu thì dùng 2 field dưới:
        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}
