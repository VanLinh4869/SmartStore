using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using shop.Data;
using shop.Models;

namespace shop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy Admin hiện tại từ Session
        private int? CurrentAdminId =>
            HttpContext.Session.GetInt32("AdminId");

        /// <summary>
        /// Chỉ cho Admin đăng nhập mới dùng được.
        /// Trả về null nếu OK, trả về RedirectToAction nếu chưa đăng nhập.
        /// </summary>
        private IActionResult? RequireAdmin()
        {
            if (!CurrentAdminId.HasValue)
            {
                // Chưa đăng nhập admin -> quay về trang login
                return RedirectToAction("Login", "Customer");
            }
            return null;
        }

        // ========================
        //  DANH SÁCH ĐƠN HÀNG
        // ========================
        // status = -1 => tất cả, 0 chờ duyệt, 1 đã duyệt, 2 đã giao, 3 đã hủy
        public async Task<IActionResult> Index(int status = -1)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var query = _context.Hoadons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.MaNvDuyetNavigation)
                .OrderByDescending(h => h.Ngay)
                .AsQueryable();

            if (status >= 0)
            {
                query = query.Where(h => h.TrangThai == status);
            }

            var list = await query.ToListAsync();
            ViewBag.Status = status;
            return View(list);   // Views/Orders/Index.cshtml
        }

        // ========================
        //  CHI TIẾT ĐƠN HÀNG
        // ========================
        public async Task<IActionResult> Details(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var hd = await _context.Hoadons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.MaNvDuyetNavigation)
                .Include(h => h.Cthoadons)
                    .ThenInclude(ct => ct.MaMhNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hd == null) return NotFound();
            return View(hd);
        }

        // ========================
        //  DUYỆT ĐƠN
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var hd = await _context.Hoadons.FindAsync(id);
            if (hd == null) return NotFound();

            if (hd.TrangThai == 0) // 0 = Chờ duyệt
            {
                hd.TrangThai = 1;                    // 1 = Đã duyệt
                hd.MaNvDuyet = CurrentAdminId.Value; // Lưu nhân viên duyệt
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ĐÁNH DẤU ĐÃ GIAO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var hd = await _context.Hoadons.FindAsync(id);
            if (hd == null) return NotFound();

            hd.TrangThai = 2; // 2 = ĐÃ GIAO
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Đã cập nhật đơn #{hd.MaHd} là ĐÃ GIAO.";
            return RedirectToAction("Details", new { id = hd.MaHd });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var hd = await _context.Hoadons
                .Include(h => h.Cthoadons)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hd == null) return NotFound();

            // Nếu đã hủy rồi không cộng lần 2
            if (hd.TrangThai == 3)
            {
                TempData["Message"] = $"Đơn #{hd.MaHd} đã hủy trước đó.";
                return RedirectToAction("Details", new { id });
            }

            // Hoàn kho
            foreach (var itemCT in hd.Cthoadons)
            {
                var sp = await _context.Mathangs.FindAsync(itemCT.MaMh);
                if (sp != null)
                {
                    short ton = sp.SoLuong ?? 0;     // short?
                    int luotMua = sp.LuotMua ?? 0;   // int?

                    int qty = itemCT.SoLuong ?? 0;

                    ton += (short)qty;   // tăng lại hàng
                    luotMua -= qty;      // giảm lượt mua
                    if (luotMua < 0) luotMua = 0;

                    sp.SoLuong = ton;
                    sp.LuotMua = luotMua;
                }
            }

            hd.TrangThai = 3; // ĐÃ HỦY
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Đã hủy đơn #{hd.MaHd} và hoàn kho.";
            return RedirectToAction("Index");
        }


    }
}
