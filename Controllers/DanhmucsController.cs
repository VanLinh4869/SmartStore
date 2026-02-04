using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using shop.Data;
using shop.Models;

namespace shop.Controllers
{
    public class DanhmucsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhmucsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danhmucs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Danhmucs.ToListAsync());
        }

        // GET: Danhmucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _context.Danhmucs
                .FirstOrDefaultAsync(m => m.MaDm == id);
            if (danhmuc == null)
            {
                return NotFound();
            }

            return View(danhmuc);
        }

        // GET: Danhmucs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Danhmucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDm,Ten")] Danhmuc danhmuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhmuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danhmuc);
        }

        // GET: Danhmucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _context.Danhmucs.FindAsync(id);
            if (danhmuc == null)
            {
                return NotFound();
            }
            return View(danhmuc);
        }

        // POST: Danhmucs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDm,Ten")] Danhmuc danhmuc)
        {
            if (id != danhmuc.MaDm)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhmuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhmucExists(danhmuc.MaDm))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(danhmuc);
        }

        // GET: Danhmucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _context.Danhmucs
                .FirstOrDefaultAsync(m => m.MaDm == id);
            if (danhmuc == null)
            {
                return NotFound();
            }

            return View(danhmuc);
        }

        // POST: Danhmucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // nếu có check admin thì giữ nguyên
            // if (!IsAdmin()) return RedirectToAction("Login", "Customer");

            // 1. kiểm tra xem còn sản phẩm dùng danh mục này không
            bool hasProducts = await _context.Mathangs.AnyAsync(m => m.MaDm == id);
            if (hasProducts)
            {
                TempData["Error"] = "Danh mục vẫn còn sản phẩm. Vui lòng xoá hoặc chuyển danh mục của các sản phẩm trước khi xoá danh mục này.";
                return RedirectToAction(nameof(Index));
            }

            var danhmuc = await _context.Danhmucs.FindAsync(id);
            if (danhmuc == null) return NotFound();

            _context.Danhmucs.Remove(danhmuc);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Đã xoá danh mục \"{danhmuc.Ten}\".";
            return RedirectToAction(nameof(Index));
        }


        private bool DanhmucExists(int id)
        {
            return _context.Danhmucs.Any(e => e.MaDm == id);
        }
    }
}
