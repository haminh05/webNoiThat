using Microsoft.AspNetCore.Mvc;
using cnpm.Models;
using cnpm.ViewModels;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace cnpm.Controllers
{
    public class AccountController : Controller
    {
        private readonly BiaContext _context;

        public AccountController(BiaContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (model.Username.Contains(" "))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập không được chứa khoảng trắng.");
            }
            // Kiểm tra định dạng email hợp lệ
            if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Định dạng email không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Username đã tồn tại");
                    return View();
                }
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    Role = "Customer"
                };

                try
                {
                    _context.Users.Add(user);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Đăng ký thành công!";
                    return RedirectToAction("Login");
                }
                catch
                {
                    ModelState.AddModelError("", "Có lỗi khi đăng ký, vui lòng thử lại!");
                    return View(model);
                }
            }
            return View();
        }
        private bool IsValidEmail(string email)
        {
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, emailRegex);
        }
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
                if (user == null || user.PasswordHash != HashPassword(model.Password))
                {
                    ViewData["ErrorMessage"] = "Tài khoản hoặc mật khẩu không đúng.";
                    return View(model);
                }
                if (user.Role == "Employee")
                {
                    var employee = _context.Employees.FirstOrDefault(e => e.UserId == user.UserId);
                    if (employee != null && !employee.IsActive)
                    {
                        ViewData["ErrorMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với shop để biết chi tiết";
                        return View(model);
                    }
                }
                // Lưu thông tin đăng nhập vào session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);


                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        // GET: /Account/Manage (Danh sách tài khoản)
        public IActionResult Manage(string search = "", string roleFilter = "", int page = 1)
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để truy cập.";
                return RedirectToAction("Login");
            }

            int pageSize = 10;
            var query = _context.Users
                .Include(u => u.Employees)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search) || u.Role.Contains(search));
            }

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var users = query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    FullName = u.Employees.Any() ? u.Employees.First().FullName : "",
                    PhoneNumber = u.Employees.Any() ? u.Employees.First().PhoneNumber : "",
                    IsActive = u.Role == "Employee" ? u.Employees.FirstOrDefault().IsActive : null // Chỉ áp dụng cho Employee

                }).ToList();

            // Cung cấp dữ liệu cho dropdown
            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.RoleList = new SelectList(new[]
            {
                new { Value = "All", Text = "Tất cả" },
                new { Value = "Admin", Text = "Admin" },
                new { Value = "Employee", Text = "Employee" },
                new { Value = "Customer", Text = "Customer" }
            }, "Value", "Text", roleFilter);

            return View(users);
        }
        [HttpGet]
        public IActionResult CreateUser(int? employeeId)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền thêm tài khoản Employee.";
                return RedirectToAction("Login");
            }

            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "ID nhân viên không hợp lệ.";
                return RedirectToAction("ManageEmployees");
            }

            var employee = _context.Employees.Find(employeeId);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                return RedirectToAction("ManageEmployees");
            }

            var model = new RegisterEmployeeViewModel
            {
                EmployeeId = employeeId.Value,
                Username = "",
                Email = "",
                Password = ""
            };

            return View(model);
        }


        // POST: /Account/CreateUser
        [HttpPost]
        public IActionResult CreateUser(RegisterEmployeeViewModel model)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền tạo tài khoản cho nhân viên.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model); // Trả về view với model có lỗi
                }
                // Kiểm tra email
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model); // Trả về view với model có lỗi
                }

                try
                {
                    // Tạo tài khoản cho nhân viên
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = HashPassword(model.Password),
                        Role = "Employee"
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges(); // Lưu tài khoản và lấy UserId

                    // Cập nhật UserId cho nhân viên
                    var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == model.EmployeeId);
                    if (employee == null)
                    {
                        ModelState.AddModelError("", "Nhân viên không tồn tại.");
                        return View(model);
                    }

                    employee.UserId = user.UserId; // Gán UserId cho nhân viên
                    _context.SaveChanges(); // Cập nhật nhân viên

                    TempData["SuccessMessage"] = "Tạo tài khoản cho nhân viên thành công!";
                    return RedirectToAction("ManageEmployees","Admin");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi khi tạo tài khoản: {ex.Message}");
                    return View(model);
                }
            }
            return View(model); // Trả về view nếu ModelState không hợp lệ
        }




        [HttpPost]
        public IActionResult DeleteAccount(int id)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền xóa nhân viên.";
                return RedirectToAction("Login", "Account");
            }


            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản liên quan không tồn tại.";
                return RedirectToAction("Manage","Account");
            }
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa chính mình!";
                return RedirectToAction("ManageEmployees");
            }

            try
            {
                _context.Users.Remove(user);            
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi xóa tài khoản: {ex.Message}";
            }

            return RedirectToAction("Manage","Account");
        }


        // Hàm băm mật khẩu
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Hàm kiểm tra đăng nhập
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        // Hàm kiểm tra vai trò Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }
    }
}
