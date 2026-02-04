using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace shop.Models;

[Table("KHACHHANG")]
public partial class Khachhang
{
    [Key]
    [Column("MaKH")]
    public int MaKh { get; set; }

    [StringLength(100)]
    public string Ten { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? DienThoai { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MatKhau { get; set; } = string.Empty;

    [InverseProperty("MaKhNavigation")]
    public virtual ICollection<Diachi> Diachis { get; set; } = new List<Diachi>();

    [InverseProperty("MaKhNavigation")]
    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
}
