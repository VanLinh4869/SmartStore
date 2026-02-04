using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace shop.Models;

[Table("DIACHI")]
public partial class Diachi
{
    [Key]
    [Column("MaDC")]
    public int MaDc { get; set; }

    [Column("MaKH")]
    public int MaKh { get; set; }

    [Column("DiaChi")]
    [StringLength(100)]
    public string DiaChi1 { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? PhuongXa { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? QuanHuyen { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TinhThanh { get; set; }

    public int? MacDinh { get; set; }

    [ForeignKey("MaKh")]
    [InverseProperty("Diachis")]
    public virtual Khachhang MaKhNavigation { get; set; } = null!;
}
