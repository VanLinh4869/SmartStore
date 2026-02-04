using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using shop.Data;
using shop.Models;
using shop.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace shop.Controllers
{
    public class AdminCustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====== HÀM QUYỀN QUẢN LÝ ======
        private bool IsManager()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (!adminId.HasValue) return false;

            var nv = _context.Nhanviens
                             .Include(x => x.MaCvNavigation)
                             .FirstOrDefault(x => x.MaNv == adminId.Value);
            if (nv == null) return false;

            return string.Equals(nv.MaCvNavigation?.Ten,
                                 "Quản lý",
                                 StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult? RequireManager()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Customer");

            return null;
        }

        // ====== DANH SÁCH KHÁCH HÀNG ======
        public async Task<IActionResult> Index()
        {
            var check = RequireManager();
            if (check != null) return check;

            var list = await _context.Khachhangs
                .OrderBy(k => k.MaKh)
                .ToListAsync();

            return View(list);
        }

        // ====== CHI TIẾT KHÁCH HÀNG ======
        public async Task<IActionResult> Details(int id)
        {
            var check = RequireManager();
            if (check != null) return check;

            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh == null) return NotFound();

            // đếm đơn hàng để hiển thị
            ViewBag.OrderCount = await _context.Hoadons
                .CountAsync(h => h.MaKh == id);

            return View(kh);
        }

        // ====== SỬA KHÁCH HÀNG ======
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var check = RequireManager();
            if (check != null) return check;

            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh == null) return NotFound();

            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Khachhang model)
        {
            var check = RequireManager();
            if (check != null) return check;

            if (id != model.MaKh) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh == null) return NotFound();

            kh.Ten = model.Ten;
            kh.DienThoai = model.DienThoai;
            kh.Email = model.Email;

            // có thể cho reset mật khẩu nếu nhập ô NewPassword chẳng hạn,
            // ở đây tạm giữ nguyên mật khẩu cũ
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ====== ĐƠN HÀNG CỦA KHÁCH ======
        public async Task<IActionResult> Orders(int id)
        {
            var check = RequireManager();
            if (check != null) return check;

            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh == null) return NotFound();

            var orders = await _context.Hoadons
                .Include(h => h.Cthoadons)
                    .ThenInclude(ct => ct.MaMhNavigation)
                .Where(h => h.MaKh == id)
                .OrderByDescending(h => h.Ngay)
                .ToListAsync();

            ViewBag.Customer = kh;
            return View(orders);   // Views/AdminCustomers/Orders.cshtml
        }

        // ====== XOÁ KHÁCH HÀNG ======
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var check = RequireManager();
            if (check != null) return check;

            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh == null) return NotFound();

            int orderCount = await _context.Hoadons
                .CountAsync(h => h.MaKh == id);

            ViewBag.OrderCount = orderCount;
            ViewBag.CanDelete = orderCount == 0;

            return View(kh);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var check = RequireManager();
            if (check != null) return check;

            int orderCount = await _context.Hoadons
                .CountAsync(h => h.MaKh == id);

            if (orderCount > 0)
            {
                TempData["Error"] =
                    $"Không thể xoá khách hàng vì đã có {orderCount} đơn hàng.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh != null)
            {
                _context.Khachhangs.Remove(kh);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
