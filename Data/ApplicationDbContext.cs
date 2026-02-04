using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using shop.Models;

namespace shop.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chucvu> Chucvus { get; set; }

    public virtual DbSet<Cthoadon> Cthoadons { get; set; }

    public virtual DbSet<Cuahang> Cuahangs { get; set; }

    public virtual DbSet<Danhmuc> Danhmucs { get; set; }

    public virtual DbSet<Thongso> Thongsos { get; set; }

    public virtual DbSet<Diachi> Diachis { get; set; }

    public virtual DbSet<Hoadon> Hoadons { get; set; }

    public virtual DbSet<Khachhang> Khachhangs { get; set; }

    public virtual DbSet<Mathang> Mathangs { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LIB-SV313\\SQLEXPRESS;Initial Catalog=shop;User ID=sa;Password=sql2017;TrustServerCertificate=True;");*/
    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlServer(
        "Server=MT0327\\SQLEXPRESS;Database=shop;Integrated Security=True;TrustServerCertificate=True;");*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chucvu>(entity =>
        {
            entity.HasKey(e => e.MaCv).HasName("PK__CHUCVU__27258E767BB96F3C");

            entity.Property(e => e.HeSo).HasDefaultValue(1.0);
        });

        modelBuilder.Entity<Cthoadon>(entity =>
        {
            entity.HasKey(e => e.MaCthd).HasName("PK__CTHOADON__1E4FA7714CAE14F0");

            entity.Property(e => e.DonGia).HasDefaultValue(0);
            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.Cthoadons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CTHOADON__MaHD__571DF1D5");

            entity.HasOne(d => d.MaMhNavigation).WithMany(p => p.Cthoadons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CTHOADON__MaMH__5812160E");
        });

        modelBuilder.Entity<Cuahang>(entity =>
        {
            entity.HasKey(e => e.MaCh).HasName("PK__CUAHANG__27258E00301D46A3");
        });

        modelBuilder.Entity<Danhmuc>(entity =>
        {
            entity.HasKey(e => e.MaDm).HasName("PK__DANHMUC__2725866E04D5B41B");
        });

        modelBuilder.Entity<Diachi>(entity =>
        {
            entity.HasKey(e => e.MaDc).HasName("PK__DIACHI__27258664E7124873");

            entity.Property(e => e.MacDinh).HasDefaultValue(1);
            entity.Property(e => e.PhuongXa).HasDefaultValue("Đông Xuyên");
            entity.Property(e => e.QuanHuyen).HasDefaultValue("TP. Long Xuyên");
            entity.Property(e => e.TinhThanh).HasDefaultValue("An Giang");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Diachis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DIACHI__MaKH__4AB81AF0");
        });

        modelBuilder.Entity<Hoadon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HOADON__2725A6E0AC777BC3");

            entity.Property(e => e.Ngay).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TongTien).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Hoadons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HOADON__MaKH__534D60F1");
            entity.HasOne(d => d.MaNvDuyetNavigation)
                .WithMany(p => p.HoadonsDuyet)
                .HasForeignKey(d => d.MaNvDuyet)
                .HasConstraintName("FK_HOADON_NHANVIEN_Duyet");
        });

        modelBuilder.Entity<Khachhang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KHACHHAN__2725CF1E12BE78C3");
        });

        modelBuilder.Entity<Mathang>(entity =>
        {
            entity.HasKey(e => e.MaMh).HasName("PK__MATHANG__2725DFD93A424153");

            entity.Property(e => e.GiaBan).HasDefaultValue(0);
            entity.Property(e => e.GiaGoc).HasDefaultValue(0);
            entity.Property(e => e.LuotMua).HasDefaultValue(0);
            entity.Property(e => e.LuotXem).HasDefaultValue(0);
            entity.Property(e => e.SoLuong).HasDefaultValue((short)0);

            entity.HasOne(d => d.MaDmNavigation).WithMany(p => p.Mathangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MATHANG__MaDM__3E52440B");
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__NHANVIEN__2725D70A6295ADF1");

            entity.HasOne(d => d.MaCvNavigation).WithMany(p => p.Nhanviens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NHANVIEN__MaCV__45F365D3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
