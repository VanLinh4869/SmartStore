using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shop.Models
{
    [Table("THONGSO")]
    public class Thongso
    {
        [Key]
        [Column("MaMH")]
        public int MaMh { get; set; }

        [StringLength(255)]
        public string? ManHinh { get; set; }

        [StringLength(255)]
        public string? HeDieuHanh { get; set; }

        [StringLength(500)]
        public string? CameraSau { get; set; }

        [StringLength(255)]
        public string? CameraTruoc { get; set; }

        [StringLength(255)]
        public string? CPU { get; set; }

        [StringLength(50)]
        public string? RAM { get; set; }

        [StringLength(50)]
        public string? BoNho { get; set; }

        [StringLength(255)]
        public string? TheSim { get; set; }

        [StringLength(255)]
        public string? Pin { get; set; }

        [StringLength(255)]
        public string? ThietKe { get; set; }

        // Navigation
        [ForeignKey("MaMh")]
        [InverseProperty("Thongso")]
        public virtual Mathang MaMhNavigation { get; set; } = null!;
    }
}
