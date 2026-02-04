using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace shop.Models;

[Table("CUAHANG")]
public partial class Cuahang
{
    [Key]
    [Column("MaCH")]
    public int MaCh { get; set; }

    [StringLength(100)]
    public string Ten { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? DienThoai { get; set; }

    [StringLength(100)]
    public string? DiaChi { get; set; }
}
