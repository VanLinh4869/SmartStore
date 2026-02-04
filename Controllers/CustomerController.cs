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
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CARTKEY = "CART";
        private const int PAGE_SIZE = 12;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        //  LUÔN CHẠY TRƯỚC MỖI ACTION -> SET VIEWBAG CHUNG CHO LAYOUT
        // =========================================================
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Customer login
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            ViewBag.CurrentCustomer = customerId.HasValue
                ? _context.Khachhangs.Find(customerId.Value)
                : null;

            // Admin
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            ViewBag.IsAdmin = adminId.HasValue;

            // Cart count
            var cart = GetCart();
            ViewBag.CartCount = cart.Sum(c => c.SoLuong);
        }

        // =========================================================
        //  HÀM SẮP XẾP CHUNG
        // =========================================================
        private IQueryable<Mathang> ApplySort(IQueryable<Mathang> query, string sort)
        {
            sort = (sort ?? "newest").ToLower();

            return sort switch
            {
                "price_asc" => query.OrderBy(m => m.GiaBan),
                "price_desc" => query.OrderByDescending(m => m.GiaBan),
                "newest" => query.OrderByDescending(m => m.MaMh),
                _ => query.OrderByDescending(m => m.MaMh)
            };
        }

        private async Task<List<Mathang>> Paging(IQueryable<Mathang> query, int page)
        {
            var totalItems = await query.CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = PAGE_SIZE;

            return await query
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();
        }

        // =========================================================
        //  SESSION CART HELPERS
        // =========================================================
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CARTKEY);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CARTKEY, JsonSerializer.Serialize(cart));
        }

        private bool IsStaffOrManager()
        {
            return HttpContext.Session.GetInt32("AdminId").HasValue;
        }

        private bool IsManager()
        {
            var role = HttpContext.Session.GetString("AdminRole");
            return role == "Quản lý";
        }

        // =========================================================
        //  TRANG CHỦ (LOGO ABC MOBILE)  -> CÓ BANNER + 4 MỤC
        // =========================================================
        public async Task<IActionResult> Home()
        {
            var items = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .OrderByDescending(m => m.MaMh)
                .ToListAsync();

            ViewBag.PageTitle = "Trang chủ – SmartStore";
            ViewBag.ShowBanner = true;

            return View(items); // Views/Customer/Home.cshtml
        }

        public async Task<IActionResult> Index(int? id, string? sort)
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .AsQueryable();

            // Lọc theo danh mục (iPhone, Samsung, ...)
            if (id.HasValue)
            {
                query = query.Where(m => m.MaDm == id.Value);
            }

            // Lọc / sắp xếp theo Nhu cầu
            switch (sort)
            {
                case "bestseller":    // Bán chạy
                    query = query.OrderByDescending(m => m.LuotMua);
                    break;

                case "new":          // Hàng mới về
                    query = query.OrderByDescending(m => m.MaMh);
                    break;

                case "sale":         // Giảm giá tốt
                    query = query
                        .Where(m => (m.GiaGoc ?? 0) > (m.GiaBan ?? 0))
                        .OrderByDescending(m => (m.GiaGoc ?? 0) - (m.GiaBan ?? 0));
                    break;

                default:
                    // mặc định: mới nhất trước
                    query = query.OrderByDescending(m => m.MaMh);
                    break;
            }

            var list = await query.ToListAsync();
            return View(list);   // dùng lại Views/Customer/Index.cshtml
        }

        // =========================================================
        //  ĐIỆN THOẠI (MENU) -> CHỈ HIỆN ĐIỆN THOẠI, TẮT BANNER
        //  MaDM: 1 iPhone, 2 Samsung, 3 Xiaomi
        // =========================================================
        public async Task<IActionResult> Phones(int page = 1, string sort = "newest")
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .Where(m => m.MaDm == 1 || m.MaDm == 2 || m.MaDm == 3);

            query = ApplySort(query, sort);

            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.PageTitle = "Điện thoại";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  MÁY TÍNH BẢNG (MENU) -> MaDM = 5
        // =========================================================
        public async Task<IActionResult> Tablets(int page = 1, string sort = "newest")
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .Where(m => m.MaDm == 5);

            query = ApplySort(query, sort);
            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.PageTitle = "Máy tính bảng";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  TAI NGHE (MENU) -> MaDM = 6
        // =========================================================
        public async Task<IActionResult> Headphones(int page = 1, string sort = "newest")
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .Where(m => m.MaDm == 6);

            query = ApplySort(query, sort);
            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.PageTitle = "Tai nghe";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  PHỤ KIỆN (MENU) -> MaDM = 4
        // =========================================================
        public async Task<IActionResult> Accessories(int page = 1, string sort = "newest")
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .Where(m => m.MaDm == 4);

            query = ApplySort(query, sort);
            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.PageTitle = "Phụ kiện";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  LỌC THEO HÃNG (MEGA MENU) -> vẫn dùng chung Index
        // =========================================================
        public async Task<IActionResult> ByCategory(int id, int page = 1, string sort = "newest")
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .Where(m => m.MaDm == id);

            query = ApplySort(query, sort);
            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.CategoryId = id;

            var dm = await _context.Danhmucs.FindAsync(id);
            ViewBag.PageTitle = dm?.Ten ?? "Danh mục";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  TÌM KIẾM
        // =========================================================
        public async Task<IActionResult> Search(string keyword, int page = 1, string sort = "newest")
        {
            keyword = keyword?.Trim() ?? "";

            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(m => m.Ten.Contains(keyword));

            query = ApplySort(query, sort);
            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.Keyword = keyword;
            ViewBag.PageTitle = $"Kết quả tìm kiếm: {keyword}";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  LỌC GIÁ
        // =========================================================
        public async Task<IActionResult> FilterPrice(int? min, int? max, int page = 1, string sort = "newest")
        {
            var query = _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .AsQueryable();

            if (min.HasValue) query = query.Where(m => m.GiaBan >= min.Value);
            if (max.HasValue) query = query.Where(m => m.GiaBan <= max.Value);

            query = ApplySort(query, sort);
            var items = await Paging(query, page);

            ViewBag.Sort = sort;
            ViewBag.MinPrice = min;
            ViewBag.MaxPrice = max;

            ViewBag.PageTitle = "Lọc theo giá";
            ViewBag.ShowBanner = false;

            return View("Index", items);
        }

        // =========================================================
        //  CHI TIẾT
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mathang = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);

            if (mathang == null) return NotFound();

            var thongSo = await _context.Thongsos
                .FirstOrDefaultAsync(t => t.MaMh == mathang.MaMh);

            var vm = new ProductDetailViewModel
            {
                Product = mathang,
                Specs = thongSo,
                MoTaChiTiet = mathang.MoTa ?? string.Empty
            };

            // Cấu hình màu & bộ nhớ demo theo danh mục
            switch (mathang.MaDm)
            {
                case 1: // iPhone
                    vm.AvailableColors = new() { "Đen", "Trắng", "Xanh", "Hồng" };
                    vm.AvailableStorages = new() { "128GB", "256GB", "512GB" };
                    break;
                case 2: // Samsung
                    vm.AvailableColors = new() { "Đen", "Xanh", "Tím" };
                    vm.AvailableStorages = new() { "128GB", "256GB" };
                    break;
                case 3: // Xiaomi
                    vm.AvailableColors = new() { "Đen", "Trắng", "Xanh" };
                    vm.AvailableStorages = new() { "256GB", "512GB" };
                    break;
                default:
                    vm.AvailableColors = new() { "Mặc định" };
                    vm.AvailableStorages = new() { "Mặc định" };
                    break;
            }

            // Sản phẩm tương tự cùng danh mục
            vm.RelatedProducts = await _context.Mathangs
                .Where(m => m.MaDm == mathang.MaDm && m.MaMh != mathang.MaMh)
                .OrderByDescending(m => m.MaMh)
                .Take(4)
                .ToListAsync();

            return View(vm);
        }

        // =========================================================
        //  GIỎ HÀNG – THÊM SẢN PHẨM (có màu / bộ nhớ)
        // =========================================================

        // Dùng chung logic cho GET + POST
        private IActionResult AddToCartInternal(int id, string? color, string? storage, int quantity)
        {
            var product = _context.Mathangs.Find(id);
            if (product == null) return NotFound();

            var cart = GetCart();   // hàm cũ của bạn

            // phân biệt theo màu + bộ nhớ
            var item = cart.FirstOrDefault(c =>
                c.MatHang.MaMh == id &&
                c.MauSac == (color ?? "") &&
                c.PhienBan == (storage ?? "")
            );

            if (item == null)
            {
                item = new CartItem
                {
                    MatHang = product,
                    SoLuong = quantity,
                    MauSac = color ?? "",
                    PhienBan = storage ?? ""
                };
                cart.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            SaveCart(cart);         // hàm cũ của bạn
            return RedirectToAction("Cart");
        }

        // --- POST: dùng ở trang chi tiết (có chọn màu / bộ nhớ) ---
        [HttpPost]
        public IActionResult AddToCart(int id, string? color, string? storage, int quantity = 1)
        {
            return AddToCartInternal(id, color, storage, quantity);
        }

        // --- GET: dùng cho nút "Mua ngay" ở danh sách (không chọn màu / bộ nhớ) ---
        [HttpGet]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            return AddToCartInternal(id, null, null, quantity);
        }

        public IActionResult Cart()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(c => c.ThanhTien);
            ViewBag.PageTitle = "Giỏ hàng";
            ViewBag.ShowBanner = false;
            return View(cart);
        }

        public IActionResult RemoveItem(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MatHang.MaMh == id);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult UpdateItem(int id, int quantity, string? color, string? storage)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MatHang.MaMh == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.SoLuong = quantity;
                    item.MauSac = color;
                    item.PhienBan = storage;
                }
            }

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        public IActionResult ClearCart()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("Cart");
        }

        // =========================
        //  MUA NGAY 1 SẢN PHẨM -> GIỎ HÀNG
        // =========================
        public async Task<IActionResult> BuyNow(int id)
        {
            // phải đăng nhập khách hàng
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var product = await _context.Mathangs.FindAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();

            // Ở đây chưa chọn màu / bộ nhớ nên để null
            var item = cart.FirstOrDefault(c => c.MatHang.MaMh == id);
            if (item == null)
            {
                item = new CartItem
                {
                    MatHang = product,
                    SoLuong = 1,
                    MauSac = null,
                    PhienBan = null
                };
                cart.Add(item);
            }
            else
            {
                item.SoLuong += 1;
            }

            SaveCart(cart);

            // -> đi tới giỏ hàng
            return RedirectToAction("Cart");
        }

        // =============================
        //  THANH TOÁN
        // =============================

        [HttpGet]
        public IActionResult CheckOut()
        {
            // Nếu chưa đăng nhập -> bắt đăng nhập
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
            {
                TempData["ReturnUrl"] = Url.Action("CheckOut");
                return RedirectToAction("Login");
            }

            var cart = GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Cart");

            var kh = _context.Khachhangs.Find(customerId.Value);

            var vm = new CheckoutViewModel
            {
                FullName = kh?.Ten ?? "",
                Phone = kh?.DienThoai ?? "",
                Address = "",          // cho người dùng tự nhập
                Items = cart,
                Total = cart.Sum(c => (int)c.ThanhTien)
            };

            ViewBag.PageTitle = "Thanh toán";
            ViewBag.ShowBanner = false;

            return View(vm);          // Views/Customer/CheckOut.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckOut(CheckoutViewModel model)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login");

            var cart = GetCart();

            // Validate địa chỉ
            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError("Address", "Địa chỉ bắt buộc nhập");
            }

            if (!ModelState.IsValid)
            {
                model.Items = cart;
                model.Total = cart.Sum(c => (int)c.ThanhTien);
                return View("CheckOut", model);
            }

            // 1. Tạo hóa đơn (Trạng thái: 0 = CHỜ DUYỆT)
            var hoaDon = new Hoadon
            {
                MaKh = customerId.Value,
                Ngay = DateTime.Now,
                TongTien = cart.Sum(c => (int)c.ThanhTien),
                TrangThai = 0,
                MaNvDuyet = null,
                NgayDuyet = null
            };

            _context.Hoadons.Add(hoaDon);
            await _context.SaveChangesAsync();   // có MaHd

            // 2. Thêm CTHOADON + cập nhật tồn kho & lượt mua
            foreach (var item in cart)
            {
                // chi tiết hóa đơn
                var ct = new Cthoadon
                {
                    MaHd = hoaDon.MaHd,
                    MaMh = item.MatHang.MaMh,
                    SoLuong = item.SoLuong,
                    DonGia = item.MatHang.GiaBan ?? 0,
                    ThanhTien = (item.MatHang.GiaBan ?? 0) * item.SoLuong
                };
                _context.Cthoadons.Add(ct);

                // cập nhật bảng MATHANG
                var sp = await _context.Mathangs.FindAsync(item.MatHang.MaMh);
                if (sp != null)
                {
                    short ton = sp.SoLuong ?? 0;    // SoLuong là short?
                    int luotMua = sp.LuotMua ?? 0;  // LuotMua là int?

                    int qty = item.SoLuong;

                    ton -= (short)qty;
                    if (ton < 0) ton = 0;

                    luotMua += qty;

                    sp.SoLuong = ton;      // OK short
                    sp.LuotMua = luotMua;  // OK int?
                }

            }

            await _context.SaveChangesAsync();

            // Xoá giỏ hàng
            SaveCart(new List<CartItem>());

            return RedirectToAction("OrderSuccess", new { id = hoaDon.MaHd });
        }

        public async Task<IActionResult> OrderSuccess(int id)
        {
            var hd = await _context.Hoadons
                .Include(h => h.MaKhNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hd == null) return NotFound();

            return View(hd);   // Views/Customer/OrderSuccess.cshtml
        }

        // =========================================================
        //  LOGIN / LOGOUT
        // =========================================================
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.PageTitle = "Đăng nhập";
            ViewBag.ShowBanner = false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            email = email?.Trim() ?? "";
            password = password?.Trim() ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return View();
            }

            var admin = await _context.Nhanviens
                .Include(n => n.MaCvNavigation)
                .FirstOrDefaultAsync(n => n.Email == email && n.MatKhau == password);

            if (admin != null && admin.MaCvNavigation != null)
            {
                HttpContext.Session.SetInt32("AdminId", admin.MaNv);
                HttpContext.Session.SetString("AdminRole", admin.MaCvNavigation.Ten);
                HttpContext.Session.Remove("CustomerId");
                return RedirectToAction("Home");
            }


            var kh = await _context.Khachhangs
                .FirstOrDefaultAsync(k => k.Email == email && k.MatKhau == password);

            if (kh == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            HttpContext.Session.SetInt32("CustomerId", kh.MaKh);
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminRole");
            return RedirectToAction("Home");

        }

        public IActionResult Signout()
        {
            HttpContext.Session.Remove("CustomerId");
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminRole");
            return RedirectToAction("Home");
        }

        // GET: Customer/Register
        [HttpGet]
        public IActionResult Register()
        {
            // nếu đã đăng nhập rồi thì về trang chủ
            if (HttpContext.Session.GetInt32("CustomerId").HasValue)
                return RedirectToAction("Home");

            return View();
        }

        // POST: Customer/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Ten,DienThoai,Email,MatKhau")] Khachhang model)
        {
            if (string.IsNullOrWhiteSpace(model.Ten))
                ModelState.AddModelError("Ten", "Họ tên không được để trống");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email không được để trống");
            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError("MatKhau", "Mật khẩu không được để trống");

            if (ModelState.IsValid)
            {
                bool emailExists = await _context.Khachhangs
                    .AnyAsync(k => k.Email == model.Email);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Khachhangs.Add(model);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("CustomerId", model.MaKh);
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminRole");

            return RedirectToAction("Home");
        }

        // ====================== HỒ SƠ KHÁCH HÀNG ======================

        [HttpGet]
        public async Task<IActionResult> CustomerInfo()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login");

            var kh = await _context.Khachhangs.FindAsync(customerId.Value);
            if (kh == null)
                return NotFound();

            var vm = new CustomerInfoViewModel
            {
                MaKh = kh.MaKh,
                Ten = kh.Ten,
                DienThoai = kh.DienThoai,
                Email = kh.Email
            };

            ViewBag.PageTitle = "Hồ sơ của tôi";
            ViewBag.ShowBanner = false;

            return View(vm);                   // Views/Customer/CustomerInfo.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerInfo(CustomerInfoViewModel model)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                ViewBag.PageTitle = "Hồ sơ của tôi";
                ViewBag.ShowBanner = false;
                return View(model);
            }

            var kh = await _context.Khachhangs.FindAsync(customerId.Value);
            if (kh == null)
                return NotFound();

            // Cập nhật thông tin
            kh.Ten = model.Ten;
            kh.DienThoai = model.DienThoai;
            kh.Email = model.Email;

            // Nếu có đổi mật khẩu
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                kh.MatKhau = model.NewPassword.Trim();
            }

            await _context.SaveChangesAsync();

            ViewBag.PageTitle = "Hồ sơ của tôi";
            ViewBag.ShowBanner = false;
            ViewBag.Success = "Cập nhật hồ sơ thành công.";

            return View(model);
        }

        // ====================== HỒ SƠ Quản lý ======================

        [HttpGet]
        public async Task<IActionResult> AdminInfo()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (!adminId.HasValue)
            {
                // Không phải admin => đá về login
                return RedirectToAction("Login");
            }

            var nv = await _context.Nhanviens
                .Include(n => n.MaCvNavigation)
                .FirstOrDefaultAsync(n => n.MaNv == adminId.Value);

            if (nv == null)
            {
                return RedirectToAction("Login");
            }

            return View(nv);
        }

        // =======================
        //  ĐƠN HÀNG CỦA TÔI (KHÁCH HÀNG)
        // =======================
        public async Task<IActionResult> MyOrders(int status = -1)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var query = _context.Hoadons
                .Include(h => h.Cthoadons)
                    .ThenInclude(ct => ct.MaMhNavigation)
                .Where(h => h.MaKh == customerId.Value)
                .OrderByDescending(h => h.Ngay)
                .AsQueryable();

            if (status >= 0)
            {
                query = query.Where(h => h.TrangThai == status);
            }

            var list = await query.ToListAsync();
            ViewBag.Status = status;
            ViewBag.PageTitle = "Đơn hàng của tôi";
            ViewBag.ShowBanner = false;

            return View(list);   // Views/Customer/MyOrders.cshtml
        }

    }
}

