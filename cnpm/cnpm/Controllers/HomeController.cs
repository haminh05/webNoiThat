using cnpm.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList;
using X.PagedList.Mvc.Core;
using System.Linq;
using X.PagedList.Extensions;
using System.Globalization;
using System.Text;


namespace cnpm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        BiaContext db=new BiaContext();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }


        public IActionResult Index(string query, int? page)
        {
            
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var products = db.Products.AsQueryable();
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
                    products = db.Products.Where(p =>
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
            var pagedProducts = products.OrderBy(p => p.ProductId).ToPagedList(pageNumber, pageSize);

            ViewBag.Query = query;
            ViewBag.Page = pageNumber;
            ViewBag.TotalPages = pagedProducts.PageCount;

            return View(pagedProducts);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //product detail
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await db.Products
                .Include(p => p.ProductDetail) // Load luôn thông tin chi tiết
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User) // Load cả đánh giá và người dùng đánh giá
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy sản phẩm
            }

            return View(product);
        }
        [HttpGet]
        public IActionResult ContactUs()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AboutUs()
        {
            return View();
        }
    }
}
