using System.Collections.Generic;
using shop.Models;

namespace shop.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        // Sản phẩm chính
        public Mathang Product { get; set; } = null!;

        // Thông số kỹ thuật (bảng THONGSO)
        public Thongso? Specs { get; set; }

        // Mô tả chi tiết (nếu cần dùng thêm)
        public string MoTaChiTiet { get; set; } = string.Empty;

        // Danh sách sản phẩm tương tự
        public List<Mathang> RelatedProducts { get; set; } = new();

        // Các lựa chọn màu sắc & bộ nhớ (để vẽ nút chọn)
        public List<string> AvailableColors { get; set; } = new();
        public List<string> AvailableStorages { get; set; } = new();
    }
}
