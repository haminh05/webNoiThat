using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cnpm.Models;
using cnpm.ViewModels;
using cnpm.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace cnpm.Controllers
{
    //[Authorize] // Yêu cầu đăng nhập
    public class OrderController : Controller
    {
        private readonly BiaContext _context;
        private const string CartSessionKey = "Cart";

        public OrderController(BiaContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ session
        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        }

        // Xử lý đặt hàng
        [HttpPost]
        public IActionResult PlaceOrder(UserInformationViewModel userInfo)
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userInfo.UserInformationId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không hợp lệ!";
                return RedirectToAction("Index", "Cart");
            }

            // Lưu thông tin nhận hàng
            var userInformation = new UserInformation
            {
                UserId = user.UserId,
                FullName = userInfo.FullName,
                PhoneNumber = userInfo.PhoneNumber,
                ShippingAddress = userInfo.ShippingAddress
            };
            _context.UserInformations.Add(userInformation);
            _context.SaveChanges();

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = user.UserId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalPrice = cart.Sum(c => c.Quantity * c.Price)
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Thêm sản phẩm vào OrderDetails
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }
            _context.SaveChanges();

            // Xóa giỏ hàng sau khi đặt hàng thành công
            HttpContext.Session.Remove(CartSessionKey);

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Checkout(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index", "Home");
            }
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userInfo = _context.UserInformations.FirstOrDefault(u => u.UserId == userId);

            var model = new CheckoutViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1,
                UserInformation = userInfo != null
         ? new UserInformationViewModel
         {
             FullName = userInfo.FullName,
             PhoneNumber = userInfo.PhoneNumber,
             ShippingAddress = userInfo.ShippingAddress
         }
         : new UserInformationViewModel()
            };


            return View(model);
        }
        [HttpPost]
        public IActionResult ConfirmOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Lưu thông tin nhận hàng
            var userInfo = _context.UserInformations.FirstOrDefault(u => u.UserId == userId);
            if (userInfo == null)
            {
                userInfo = new UserInformation
                {
                    UserId = userId,
                    FullName = model.UserInformation.FullName,
                    PhoneNumber = model.UserInformation.PhoneNumber,
                    ShippingAddress = model.UserInformation.ShippingAddress
                };
                _context.UserInformations.Add(userInfo);
                _context.SaveChanges();
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalPrice = model.Price * model.Quantity
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Thêm sản phẩm vào OrderDetails
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = model.ProductId,
                Quantity = model.Quantity,
                UnitPrice = model.Price
            };
            _context.OrderDetails.Add(orderDetail);
            _context.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Orders");
        }

    }
}