using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace shop.Models;

[Table("CTHOADON")]
public partial class Cthoadon
{
    [Key]
    [Column("MaCTHD")]
    public int MaCthd { get; set; }

    [Column("MaHD")]
    public int MaHd { get; set; }

    [Column("MaMH")]
    public int MaMh { get; set; }

    public int? DonGia { get; set; }

    public int? SoLuong { get; set; }

    public int? ThanhTien { get; set; }

    [ForeignKey("MaHd")]
    [InverseProperty("Cthoadons")]
    public virtual Hoadon MaHdNavigation { get; set; } = null!;

    [ForeignKey("MaMh")]
    [InverseProperty("Cthoadons")]
    public virtual Mathang MaMhNavigation { get; set; } = null!;
}
