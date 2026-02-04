using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class NhanviensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private bool IsAdmin()
        {
            return HttpContext.Session.GetInt32("AdminId").HasValue;
        }
        private bool IsManager()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (!adminId.HasValue) return false;

            var nv = _context.Nhanviens
                             .Include(x => x.MaCvNavigation)
                             .FirstOrDefault(x => x.MaNv == adminId.Value);

            if (nv == null) return false;

            // Tên chức vụ “Quản lý” đúng như bạn đặt trong bảng CHUCVU
            return string.Equals(nv.MaCvNavigation?.Ten,
                                 "Quản lý",
                                 StringComparison.OrdinalIgnoreCase);
        }

        public NhanviensController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Nhanviens
        public async Task<IActionResult> Index()
        {
            if (!IsManager()) return RedirectToAction("Login", "Customer");

            var applicationDbContext = _context.Nhanviens.Include(n => n.MaCvNavigation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Nhanviens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens
                .FirstOrDefaultAsync(n => n.MaNv == id);

            if (nhanvien == null)
            {
                return NotFound();
            }

            return View(nhanvien);
        }


        // GET: Nhanviens/Create
        public IActionResult Create()
        {
            if (!IsManager()) return RedirectToAction("Login", "Customer");

            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "Ten");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nhanvien nhanvien)
        {
            // Mật khẩu mặc định nếu bỏ trống
            if (string.IsNullOrWhiteSpace(nhanvien.MatKhau))
            {
                nhanvien.MatKhau = "123";
            }

            // Bỏ validation cho navigation property
            ModelState.Remove("MaCvNavigation");

            if (ModelState.IsValid)
            {
                _context.Add(nhanvien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "Ten", nhanvien.MaCv);
            return View(nhanvien);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsManager()) return RedirectToAction("Login", "Customer");

            if (id == null) return NotFound();
            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv == null) return NotFound();

            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "Ten", nv.MaCv);
            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Nhanvien model)
        {
            if (!IsManager()) return RedirectToAction("Login", "Customer");
            if (id != model.MaNv) return NotFound();

            // Bỏ validate navigation
            ModelState.Remove("MaCvNavigation");

            if (!ModelState.IsValid)
            {
                ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "Ten", model.MaCv);
                return View(model);
            }

            // Lấy nhân viên hiện tại trong DB
            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv == null) return NotFound();

            // Kiểm tra xem có đổi chức vụ không
            bool roleChanged = nv.MaCv != model.MaCv;

            // Cập nhật các thuộc tính được phép sửa
            nv.Ten = model.Ten;
            nv.Email = model.Email;
            nv.DienThoai = model.DienThoai;
            nv.MaCv = model.MaCv;

            // Nếu ĐỔI chức vụ -> reset mật khẩu về 123
            if (roleChanged)
            {
                nv.MatKhau = "123";
            }

            await _context.SaveChangesAsync();

            // Nếu đang sửa chính tài khoản đang đăng nhập thì cập nhật lại AdminRole trong session
            var currentAdminId = HttpContext.Session.GetInt32("AdminId");
            if (currentAdminId.HasValue && currentAdminId.Value == nv.MaNv)
            {
                var cv = await _context.Chucvus.FindAsync(nv.MaCv);
                HttpContext.Session.SetString("AdminRole", cv?.Ten ?? "");
            }

            return RedirectToAction(nameof(Index));
        }



        // GET: Nhanviens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            if (id == null) return NotFound();

            var nv = await _context.Nhanviens
                .Include(n => n.MaCvNavigation)
                .FirstOrDefaultAsync(m => m.MaNv == id);

            if (nv == null) return NotFound();

            // Đếm số hóa đơn do nhân viên này duyệt
            int orderCount = await _context.Hoadons
                .CountAsync(h => h.MaNvDuyet == id);

            ViewBag.OrderCount = orderCount;
            ViewBag.CanDelete = orderCount == 0;

            return View(nv);
        }


        // POST: Nhanviens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            // Kiểm tra lại để chắc chắn
            int orderCount = await _context.Hoadons
                .CountAsync(h => h.MaNvDuyet == id);

            if (orderCount > 0)
            {
                TempData["ErrorMessage"] =
                    $"Không thể xóa nhân viên vì đã duyệt {orderCount} hóa đơn. " +
                    "Hãy chuyển người duyệt khác cho các hóa đơn này trước.";

                return RedirectToAction(nameof(Delete), new { id });
            }

            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv != null)
            {
                _context.Nhanviens.Remove(nv);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool NhanvienExists(int id)
        {
            return _context.Nhanviens.Any(e => e.MaNv == id);
        }
    }
}
