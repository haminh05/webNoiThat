using cnpm.Helpers;
using cnpm.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace cnpm.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";

        // Lấy giỏ hàng từ session
        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        }

        // Lưu giỏ hàng vào session
        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            return View(GetCart());
        }

        // Thêm sản phẩm vào giỏ hàng (AJAX)
      
        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItemViewModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
            }

            

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == model.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(model);
            }

            SaveCart(cart);

            return Json(new { success = true, cartItemCount = cart.Sum(i => i.Quantity) });
        }


        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng sản phẩm
        [HttpPost]
        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == id);

            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }
        //lấy số sp trong giỏ hàng bann đầu
        public IActionResult GetCartItemCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            return Json(new { cartItemCount = cart.Sum(i => i.Quantity) });
        }

        // Thanh toán sản phẩm đã chọn
  
        [HttpPost]
        public IActionResult CheckoutSelected(string selectedProducts)
        {
            if (string.IsNullOrEmpty(selectedProducts))
            {
                TempData["Error"] = "Bạn chưa chọn sản phẩm nào!";
                return RedirectToAction("Index");
            }

            var productIds = selectedProducts.Split(',').Select(int.Parse).ToArray();
            var cart = GetCart();

            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            var selectedItems = cart.Where(p => productIds.Contains(p.ProductId)).ToList();

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Không tìm thấy sản phẩm đã chọn!";
                return RedirectToAction("Index");
            }

            return View("Checkout", selectedItems);
        }


        // Thanh toán toàn bộ giỏ hàng
        [HttpPost]
        public IActionResult CheckoutAll()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            return View("Checkout", cart);
        }
    }
}
