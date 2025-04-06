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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace cnpm.Controllers
{
    public class OrdersController : Controller
    {
        private readonly BiaContext _context;
        private const string CartSessionKey = "SelectedCart";

        public OrdersController(BiaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem đơn hàng!";
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.Orders
                                 .Where(o => o.UserId == userId)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            return View(orders);
        }

        private List<CartItemViewModel> GetCart()
        {


            var cartSession = HttpContext.Session.GetString("Cart");
            //Console.WriteLine($"📌 Session Cart trước khi parse JSON: {cartSession}");

            if (string.IsNullOrEmpty(cartSession))
            {
                Console.WriteLine("⚠️ Session Cart bị mất!");
                return new List<CartItemViewModel>();
            }

            var cart = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartSession);
            //Console.WriteLine($"✅ Giỏ hàng sau khi parse: {JsonConvert.SerializeObject(cart)}");
            return cart;
        }

        /*
        [HttpGet]
        public IActionResult PlaceOrder()
        {
            
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }



            var userId = HttpContext.Session.GetInt32("UserId");
            var userInfo = _context.UserInformations.FirstOrDefault(u => u.UserId == userId);
            var model = new CartCheckoutViewModel
            {
                CartItems = cart,
                UserInformation = userInfo != null ? new UserInformationViewModel
                {
                    FullName = userInfo.FullName,
                    PhoneNumber = userInfo.PhoneNumber,
                    ShippingAddress = userInfo.ShippingAddress
                } : new UserInformationViewModel()// Có thể lấy thông tin từ DB nếu cần
            };

            return View(model);
        }
        */


        [HttpPost]
        public IActionResult PlaceOrder(UserInformationViewModel userInfo)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập để đặt hàng!";
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrWhiteSpace(userInfo.FullName) ||
                    string.IsNullOrWhiteSpace(userInfo.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(userInfo.ShippingAddress))
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ thông tin nhận hàng!";
                    return RedirectToAction("ConfirmOrder");
                }

                var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("SelectedCart");
                if (cart == null || !cart.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index", "Cart");
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Tạo đơn hàng
                        var order = new Order
                        {
                            UserId = userId.Value,
                            OrderDate = DateTime.Now,
                            Status = "Pending",
                            TotalPrice = cart.Sum(c => c.Quantity * c.Price),
                            FullName = userInfo.FullName,
                            PhoneNumber = userInfo.PhoneNumber,
                            ShippingAddress = userInfo.ShippingAddress
                        };
                        _context.Orders.Add(order);
                        _context.SaveChanges();

                        foreach (var item in cart)
                        {
                            // Lấy sản phẩm từ database
                            var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                            if (product != null)
                            {
                                // Kiểm tra xem sản phẩm có đủ hàng không
                                if (product.Stock >= item.Quantity)
                                {
                                    // Trừ số lượng tồn kho
                                    product.Stock -= item.Quantity;

                                    // Thêm chi tiết đơn hàng
                                    _context.OrderDetails.Add(new OrderDetail
                                    {
                                        OrderId = order.OrderId,
                                        ProductId = item.ProductId,
                                        Quantity = item.Quantity,
                                        UnitPrice = item.Price
                                    });
                                }
                                else
                                {
                                    TempData["Error"] = $"Sản phẩm {product.Name} không đủ số lượng!";
                                    transaction.Rollback(); // Quay lại dữ liệu ban đầu nếu lỗi
                                    return RedirectToAction("ConfirmOrder");
                                }
                            }
                            else
                            {
                                TempData["Error"] = $"Sản phẩm có ID {item.ProductId} không tồn tại!";
                                transaction.Rollback();
                                return RedirectToAction("ConfirmOrder");
                            }
                        }

                       
                        // Lưu thay đổi vào database
                        _context.SaveChanges();
                        transaction.Commit(); // Xác nhận giao dịch thành công

                        // Xóa giỏ hàng khỏi session
                        HttpContext.Session.Remove("Cart");
                        HttpContext.Session.Remove(CartSessionKey);

                        TempData["Success"] = "Đặt hàng thành công!";
                        return RedirectToAction("Index", "Orders");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Nếu có lỗi, hoàn tác tất cả thay đổi
                        TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại!";
                        Console.WriteLine($"Lỗi: {ex.Message}");
                        return RedirectToAction("ConfirmOrder");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống! Vui lòng thử lại sau.";
                Console.WriteLine($"Lỗi: {ex.Message}");
                return RedirectToAction("ConfirmOrder");
            }
        }





        [HttpGet]
        public IActionResult Checkout(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để tiếp tục thanh toán!";
                TempData.Keep("Error");
                return RedirectToAction("Login", "Account"); 
            }
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index", "Home");
            }

           
            var userInfo = _context.UserInformations.FirstOrDefault(u => u.UserId == userId);

            var model = new CheckoutViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1,
                UserInformation = userInfo != null ? new UserInformationViewModel
                {
                    FullName = userInfo.FullName,
                    PhoneNumber = userInfo.PhoneNumber,
                    ShippingAddress = userInfo.ShippingAddress
                } : new UserInformationViewModel()
            };

            return View(model);
        }
        [HttpGet]

        public IActionResult ConfirmOrder()
        {
            var selectedCartItems = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("SelectedCart");

            if (selectedCartItems == null || !selectedCartItems.Any())
            {
                TempData["Error"] = "Không có sản phẩm nào để thanh toán!";
                return RedirectToAction("Index", "Cart");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userInfo = _context.UserInformations.FirstOrDefault(u => u.UserId == userId);

            var model = new CartCheckoutViewModel
            {
                CartItems = selectedCartItems,
                UserInformation = userInfo != null ? new UserInformationViewModel
                {
                    FullName = userInfo.FullName,
                    PhoneNumber = userInfo.PhoneNumber,
                    ShippingAddress = userInfo.ShippingAddress
                } : new UserInformationViewModel()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmOrder(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Checkout", model);
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập để đặt hàng!";
                    return RedirectToAction("Login", "Account");
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Lưu thông tin nhận hàng
                        var userInfo = _context.UserInformations.FirstOrDefault(u => u.UserId == userId);
                        if (userInfo == null)
                        {
                            userInfo = new UserInformation
                            {
                                UserId = userId.Value,
                                FullName = model.UserInformation.FullName,
                                PhoneNumber = model.UserInformation.PhoneNumber,
                                ShippingAddress = model.UserInformation.ShippingAddress
                            };
                            _context.UserInformations.Add(userInfo);
                        }
                        else
                        {
                            // Cập nhật thông tin nếu đã tồn tại
                            userInfo.FullName = model.UserInformation.FullName;
                            userInfo.PhoneNumber = model.UserInformation.PhoneNumber;
                            userInfo.ShippingAddress = model.UserInformation.ShippingAddress;
                        }
                        _context.SaveChanges();

                        // Lấy sản phẩm từ database
                        var product = _context.Products.FirstOrDefault(p => p.ProductId == model.ProductId);
                        if (product == null)
                        {
                            TempData["Error"] = "Sản phẩm không tồn tại!";
                            transaction.Rollback();
                            return RedirectToAction("Checkout");
                        }

                        // Kiểm tra số lượng hàng trong kho
                        if (product.Stock < model.Quantity)
                        {
                            TempData["Error"] = $"Sản phẩm {product.Name} không đủ hàng!";
                            transaction.Rollback();
                            return RedirectToAction("Checkout");
                        }

                        // Trừ số lượng tồn kho
                        product.Stock -= model.Quantity;

                        // Tạo đơn hàng
                        var order = new Order
                        {
                            UserId = userId.Value,
                            OrderDate = DateTime.Now,
                            Status = "Pending",
                            TotalPrice = model.Price * model.Quantity,
                            FullName = userInfo.FullName,
                            PhoneNumber = userInfo.PhoneNumber,
                            ShippingAddress = userInfo.ShippingAddress
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

                        // Lưu thay đổi
                        _context.SaveChanges();
                        transaction.Commit(); // Xác nhận giao dịch thành công

                        TempData["Success"] = "Đặt hàng thành công!";
                        return RedirectToAction("Index", "Orders");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại!";
                        Console.WriteLine($"Lỗi: {ex.Message}");
                        return RedirectToAction("Checkout");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống! Vui lòng thử lại sau.";
                Console.WriteLine($"Lỗi: {ex.Message}");
                return RedirectToAction("Checkout");
            }
        }

        public async Task<IActionResult> History()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var orders = await _context.Orders
                .Where(o => o.UserId == userId && (o.Status == "Completed" || o.Status == "Cancelled"))
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var reviewedProducts = _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => r.ProductId)
                .ToList();

            ViewBag.ReviewedProducts = reviewedProducts;

            return View(orders);
        }

        public async Task<IActionResult> Reorder(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để tiếp tục!";
                return RedirectToAction("Login", "Account");
            }

            // Lấy đơn hàng cũ
            var oldOrder = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (oldOrder == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("History");
            }

            // Tạo danh sách sản phẩm tạm thời từ đơn hàng cũ
            var selectedItems = oldOrder.OrderDetails.Select(od => new CartItemViewModel
            {
                ProductId = od.ProductId,
                ProductName = od.Product.Name,
                Price = od.UnitPrice,
                Quantity = od.Quantity,
                ImageUrl = "" // Nếu có hình ảnh, thêm vào đây
            }).ToList();

            // Lưu vào Session như một giỏ hàng tạm thời
            HttpContext.Session.SetObjectAsJson("SelectedCart", selectedItems);

            // Chuyển đến trang xác nhận
            return RedirectToAction("ConfirmOrder");
        }
        //CUSTOMER HUY DON/////
        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            // Chỉ cho phép hủy nếu trạng thái là "Pending" hoặc "Shipping"
            if (order.Status == "Pending" || order.Status == "Shipping")
            {
                order.Status = "Cancelled";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng khi đã hoàn thành.";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult ConfirmReceivedOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            // Chỉ cập nhật nếu đơn hàng đang ở trạng thái "Shipping"
            if (order.Status == "Shipping")
            {
                order.Status = "Completed";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Bạn đã nhận hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xác nhận đơn hàng này.";
            }

            return RedirectToAction("Index");
        }

        /////////ADMIN/////////////////
        [HttpGet]
        public IActionResult ManageOrders(string status, string search)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                TempData["Error"] = "Bạn không có quyền truy cập!";
                return RedirectToAction("Index");
            }

            var orders = _context.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o => o.FullName.Contains(search));
            }

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }



        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string actionType)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("ManageOrders");
            }

            if (actionType == "Cancel" && (order.Status == "Pending" || order.Status == "Shipping"))
            {
                order.Status = "Cancelled";
                TempData["Success"] = "Đơn hàng đã bị hủy!";
            }
            else if (actionType == "Confirm" && order.Status == "Pending")
            {
                order.Status = "Shipping";
                TempData["Success"] = "Đơn hàng đã được xác nhận!";
            }

            _context.SaveChanges();
            return RedirectToAction("ManageOrders");
        }

        [HttpGet]
        public IActionResult OrderDetail(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem chi tiết đơn hàng!";
                return RedirectToAction("Login", "Account");
            }

            var userRole = HttpContext.Session.GetString("Role"); // Lấy role từ session

            var orderQuery = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            // Nếu là admin, lấy tất cả đơn hàng, nếu không chỉ lấy đơn của user
            if (userRole != "Admin")
            {
                orderQuery = orderQuery.Where(o => o.UserId == userId);
            }

            var order = orderQuery.FirstOrDefault(o => o.OrderId == id);
            Console.WriteLine($"Order ID: {id}");
            Console.WriteLine($"UserId: {userId}, Role: {userRole}");

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index");
            }

            return View(order);
        }






    }
}
