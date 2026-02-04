using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace shop.Models;

[Table("HOADON")]
public partial class Hoadon
{
    [Key]
    [Column("MaHD")]
    public int MaHd { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Ngay { get; set; }

    public int? TongTien { get; set; }

    [Column("MaKH")]
    public int MaKh { get; set; }

    // 0 = chờ duyệt, 1 = đã duyệt, 2 = đã giao, 3 = đã hủy
    public int? TrangThai { get; set; }

    [Column("MaNvDuyet")]
    public int? MaNvDuyet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayDuyet { get; set; }

    [InverseProperty("MaHdNavigation")]
    public virtual ICollection<Cthoadon> Cthoadons { get; set; } = new List<Cthoadon>();

    [ForeignKey("MaKh")]
    [InverseProperty("Hoadons")]
    public virtual Khachhang MaKhNavigation { get; set; } = null!;

    [ForeignKey("MaNvDuyet")]
    [InverseProperty("HoadonsDuyet")]
    public virtual Nhanvien? MaNvDuyetNavigation { get; set; }
}
