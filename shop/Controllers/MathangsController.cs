using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using shop.Data;
using shop.Models;
using Microsoft.AspNetCore.Http;


namespace shop.Controllers
{
    public class MathangsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MathangsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // GET: Mathangs
        public async Task<IActionResult> Index(int? categoryId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(m => m.MaDm == categoryId);
            }

            // gửi danh sách danh mục để làm dropdown lọc
            var danhMucList = await _context.Danhmucs
                .OrderBy(d => d.Ten)
                .ToListAsync();

            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = new SelectList(danhMucList, "MaDm", "Ten", categoryId);

            return View(await query.ToListAsync());
        }



        // GET: Mathangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            if (id == null)
            {
                return NotFound();
            }

            var mathang = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (mathang == null)
            {
                return NotFound();
            }

            return View(mathang);
        }

        // GET: Mathangs/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "Ten");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mathang mathang, IFormFile? upload)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Customer");

            ModelState.Remove("MaDmNavigation");
            ModelState.Remove("Cthoadons");
            ModelState.Remove("Thongsos");

            if (ModelState.IsValid)
            {
                if (upload != null && upload.Length > 0)
                {
                    var folder = Path.Combine(_env.WebRootPath, "images", "products");
                    Directory.CreateDirectory(folder);

                    var fileName = Path.GetFileName(upload.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await upload.CopyToAsync(stream);
                    }

                    mathang.HinhAnh = fileName;   // chỉ lưu tên file
                }

                _context.Add(mathang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "Ten", mathang.MaDm);
            return View(mathang);
        }




        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            if (id == null) return NotFound();

            var mathang = await _context.Mathangs.FindAsync(id);
            if (mathang == null) return NotFound();

            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "Ten", mathang.MaDm);
            return View(mathang);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mathang model, IFormFile? upload)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Customer");

            var sp = await _context.Mathangs.FindAsync(id);
            if (sp == null) return NotFound();

            ModelState.Remove("MaDmNavigation");
            ModelState.Remove("Cthoadons");
            ModelState.Remove("Thongsos");

            if (!ModelState.IsValid)
            {
                ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "Ten", model.MaDm);
                return View(model);
            }

            sp.Ten = model.Ten;
            sp.GiaGoc = model.GiaGoc;
            sp.GiaBan = model.GiaBan;
            sp.SoLuong = model.SoLuong;
            sp.MoTa = model.MoTa;
            sp.MaDm = model.MaDm;

            if (upload != null && upload.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(folder);

                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                sp.HinhAnh = fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }





        // GET: Mathangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            if (id == null)
            {
                return NotFound();
            }

            var mathang = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (mathang == null)
            {
                return NotFound();
            }

            return View(mathang);
        }

        // POST: Mathangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Customer");
            }

            var mathang = await _context.Mathangs.FindAsync(id);
            if (mathang != null)
            {
                _context.Mathangs.Remove(mathang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MathangExists(int id)
        {
            return _context.Mathangs.Any(e => e.MaMh == id);
        }
        private bool IsAdmin()
        {
            return HttpContext.Session.GetInt32("AdminId").HasValue;
        }

    }
}
