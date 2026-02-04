using System;

namespace shop.Models
{
    public class CartItem
    {
        public Mathang MatHang { get; set; } = null!;
        public int SoLuong { get; set; }

        // === THÊM MỚI ===
        public string? MauSac { get; set; }
        public string? PhienBan { get; set; }   // bộ nhớ

        public int DonGia => MatHang.GiaBan ?? 0;
        public int ThanhTien => DonGia * SoLuong;
    }
}
