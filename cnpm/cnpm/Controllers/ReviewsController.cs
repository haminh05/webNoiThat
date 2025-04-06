using cnpm.Models;
using cnpm.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace cnpm.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly BiaContext _context;

        public ReviewsController(BiaContext context)
        {
            _context = context;
        }
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Kiểm tra xem user đã mua sản phẩm chưa
        private bool HasUserPurchasedProduct(int userId, int productId)
        {
            return _context.OrderDetails
                .Any(od => od.ProductId == productId &&
                           _context.Orders.Any(o => o.OrderId == od.OrderId && o.UserId == userId && o.Status == "Completed"));
        }

        // Hiển thị form đánh giá sản phẩm
        [HttpGet]
        public IActionResult Create(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đánh giá sản phẩm!";
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem người dùng đã mua sản phẩm này chưa
            if (!HasUserPurchasedProduct(userId.Value, productId))
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá sản phẩm đã mua!";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Trả về form đánh giá
            return View(new ReviewViewModel { ProductID = productId });
        }
        [HttpPost]
        public JsonResult CreateAjax([FromBody] ReviewViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá!" });
            }

            if (!_context.OrderDetails.Any(od => od.ProductId == model.ProductID &&
                _context.Orders.Any(o => o.OrderId == od.OrderId && o.UserId == userId.Value && o.Status == "Completed")))
            {
                return Json(new { success = false, message = "Bạn chỉ có thể đánh giá sản phẩm đã mua!" });
            }

            if (_context.Reviews.Any(r => r.UserId == userId && r.ProductId == model.ProductID))
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này trước đó!" });
            }

            var review = new Review
            {
                UserId = userId.Value,
                ProductId = model.ProductID,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // Lưu đánh giá vào database
        [HttpPost]
        public IActionResult Create(ReviewViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đánh giá sản phẩm!";
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra nếu đã mua sản phẩm
            if (!HasUserPurchasedProduct(userId.Value, model.ProductID))
            {
                TempData["Error"] = "Bạn không thể đánh giá sản phẩm này!";
                return RedirectToAction("Details", "Products", new { id = model.ProductID });
            }

            // Kiểm tra xem đã đánh giá sản phẩm này chưa
            var existingReview = _context.Reviews.FirstOrDefault(r => r.UserId == userId && r.ProductId == model.ProductID);
            if (existingReview != null)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này trước đó!";
                return RedirectToAction("Details", "Products", new { id = model.ProductID });
            }

            // Lưu đánh giá mới vào database
            var review = new Review
            {
                UserId = userId.Value,
                ProductId = model.ProductID,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            TempData["Success"] = "Đánh giá của bạn đã được gửi thành công!";
            return RedirectToAction("Details", "Products", new { id = model.ProductID });
        }

        public IActionResult Index_Manage_Review()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

            var reviews = _context.Reviews
                .Select(r => new
                {
                    r.ReviewId,
                    r.User.Username,
                    r.Product.Name,
                    r.Rating,
                    r.Comment,
                    r.ReviewDate
                })
                .ToList();

            return View("reviews", "Admin");
        }
        [HttpPost]
        public IActionResult ToggleVisibility(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();

            review.IsVisible = !review.IsVisible;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Statistics()
        {
            if (!IsAdmin()) return Unauthorized();

            var stats = _context.Products
                .Select(p => new
                {
                    p.Name,
                    AverageRating = p.Reviews.Where(r => r.IsVisible).Average(r => (double?)r.Rating) ?? 0,
                    ReviewCount = p.Reviews.Count(r => r.IsVisible)
                }).ToList();

            ViewBag.Stats = stats;
            return View();
        }
    }
}
