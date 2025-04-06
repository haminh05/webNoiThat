using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cnpm.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;
using X.PagedList.Extensions;


namespace cnpm.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BiaContext _context;

        public ProductsController(BiaContext context)
        {
            _context = context;
        }

        // GET: Products
        public IActionResult Index(string query, int? page)
        {

            int pageSize = 6;
            int pageNumber = page ?? 1;

            var products = _context.Products.AsQueryable();

            // Nếu có query tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Trim();
                products = products.Where(p => EF.Functions.Like(p.Name, $"%{query}%") ||
                                               EF.Functions.Like(p.Description, $"%{query}%"));
            }

            // Kiểm tra nếu không có kết quả
            if (!products.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm nào phù hợp.";
                ViewBag.Query = query;
                return View();
            }

            var pagedProducts = products.OrderBy(p => p.ProductId).ToPagedList(pageNumber, pageSize);

            ViewBag.Query = query;
            ViewBag.Page = pageNumber;
            ViewBag.TotalPages = pagedProducts.PageCount;

            return View(pagedProducts);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,Price,Stock,CategoryId")] Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu có file ảnh được tải lên
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Lưu file vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn ảnh vào database
                    product.ImagePath = "/Image/" + fileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductDetail) // Đảm bảo bao gồm thông tin chi tiết sản phẩm
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Bàn" },
                new SelectListItem { Value = "2", Text = "Ghế" },
                new SelectListItem { Value = "3", Text = "Sofa" },
                new SelectListItem { Value = "4", Text = "Giường" }
            };

            ViewBag.Categories = categories;
            return View(product);
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Description,Price,Stock,CategoryId,ImagePath")] Product product, IFormFile? ImageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null)
                    {
                        return NotFound();
                    }
                    if (ImageFile == null || ImageFile.Length == 0)
                    {
                        product.ImagePath = existingProduct.ImagePath;
                    }
                    // Xử lý nếu có file ảnh mới được tải lên
                    else
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                        var fileName = ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Đảm bảo thư mục tồn tại
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh mới, sử dụng \ thay vì /
                        product.ImagePath = Path.Combine("\\Image", fileName);
                    }


                    // Cập nhật thông tin sản phẩm
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }

            return View(product);
        }



        

        // POST: Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return View("Manage");

        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }


        //MÂNAGE 
        public IActionResult Manage(string query, int? categoryId, int page = 1)
        {
            int pageSize = 15; // Số sản phẩm mỗi trang
            var products = _context.Products.AsQueryable();

            // Ánh xạ CategoryId sang tên danh mục
            var categoryMapping = new Dictionary<int, string>
            {
                { 1, "Bàn" },
                { 2, "Ghế" },
                { 3, "Sofa" },
                { 4, "Giường" }
            };

            ViewBag.CategoryMapping = categoryMapping; // Gửi dữ liệu xuống View

            // Lấy danh sách các CategoryId có trong sản phẩm
            var categoryIds = products
                .Where(p => p.CategoryId.HasValue)
                .Select(p => p.CategoryId.Value)
                .Distinct()
                .ToList();

            ViewBag.Categories = categoryIds;

            // Lọc theo CategoryId nếu có
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Trim().ToLower();
                var keywords = query.Split(' ');

                // Tạo chuỗi tìm kiếm theo dạng %ghế%gỗ%
                string joinedKeywords = $"%{string.Join("%", keywords)}%";

                // Tìm sản phẩm có chứa nguyên cụm từ
                products = products.Where(p => EF.Functions.Like(p.Name.ToLower(), joinedKeywords) ||
                                               EF.Functions.Like(p.Description.ToLower(), joinedKeywords));

                // Nếu không có kết quả, tìm từng từ riêng lẻ
                if (!products.Any())
                {
                    products = _context.Products.Where(p =>
                        keywords.All(k => EF.Functions.Like(p.Name.ToLower(), $"%{k}%") ||
                                          EF.Functions.Like(p.Description.ToLower(), $"%{k}%"))
                    );
                }

                // Nếu vẫn không có kết quả, hiển thị thông báo
                if (!products.Any())
                {
                    ViewBag.Message = "Không tìm thấy sản phẩm nào phù hợp.";
                    ViewBag.Query = query;
                    return View();
                }
            }
            // Phân trang
            var pagedProducts = products.OrderBy(p => p.ProductId).ToPagedList(page, pageSize);

            return View(pagedProducts);
        }


        //product detail
        public async Task<IActionResult> ProductDetail(int id)
        {
            // Lấy sản phẩm hiện tại
            var product = await _context.Products
                .Include(p => p.ProductDetail)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User) // Lấy thông tin người dùng đánh giá
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách sản phẩm khác (cùng danh mục)
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4)
                .ToListAsync();

            // Lấy UserID từ session
            var userId = HttpContext.Session.GetInt32("UserId");

            // Kiểm tra xem người dùng có thể đánh giá sản phẩm này không
            bool canReview = false;
            if (userId.HasValue)
            {
                canReview = _context.OrderDetails
                    .Any(od => od.ProductId == id &&
                               _context.Orders.Any(o => o.OrderId == od.OrderId &&
                                                         o.UserId == userId.Value &&
                                                         o.Status == "Completed"));

                // Kiểm tra xem đã đánh giá chưa
                var hasReviewed = _context.Reviews.Any(r => r.UserId == userId.Value && r.ProductId == id);
                if (hasReviewed)
                {
                    canReview = false; // Không cho đánh giá nếu đã đánh giá trước đó
                }
            }

            // Truyền dữ liệu vào View
            ViewBag.CanReview = canReview;
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }



        [HttpGet]
        public IActionResult ListProduct(int? category, string? query, int? page)
        {
            int pageSize = 6; // Số sản phẩm mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là 1

            var products = _context.Products.AsQueryable();

            if (category.HasValue)
            {
                products = products.Where(p => p.CategoryId == category);
            }


            var pagedProducts = products.AsNoTracking().ToPagedList(pageNumber, pageSize);

            ViewBag.Query = query;
            ViewBag.Page = pageNumber;
            ViewBag.TotalPages = pagedProducts.PageCount;
            ViewBag.Category = category;

            return View(pagedProducts);
        }


    }
}
