using cnpm.Helpers;
using cnpm.Models;
using cnpm.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace cnpm.Controllers
{
  
    public class AdminController : Controller
    {
        private readonly BiaContext _context;

        public AdminController(BiaContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewData["Title"] = "Bảng điều khiển Admin";
            return View();
        }
        // GET: /Account/AddEmployee/5 (Thêm thông tin Employee cho UserId = 5)
        public IActionResult AddEmployee(int id)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền thêm thông tin nhân viên.";
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id && u.Role == "Employee");
            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại hoặc không phải Employee.";
                return RedirectToAction("Manage");
            }

            if (_context.Employees.Any(e => e.UserId == id))
            {
                TempData["ErrorMessage"] = "Tài khoản này đã có thông tin Employee.";
                return RedirectToAction("Manage");
            }

            var model = new EmployeeViewModel
            {
                UserId = user.UserId,
                Username = user.Username ?? "Unknown" // Đảm bảo không null
            };
            return View(model);
        }


        // POST: /Account/AddEmployee
        [HttpPost]
        public IActionResult AddEmployee(EmployeeViewModel model)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền thêm thông tin nhân viên.";
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId && u.Role == "Employee");
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Tài khoản không tồn tại hoặc không phải Employee.";
                    return RedirectToAction("Manage");
                }

                if (_context.Employees.Any(e => e.UserId == model.UserId))
                {
                    ModelState.AddModelError("", "Tài khoản này đã có thông tin Employee.");
                    return View(model);
                }
                if (_context.Employees.Any(e => e.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("", "Số điện thoại đã tồn tại");
                    return View(model);
                }
                if (_context.Employees.Any(e => e.IdentityCard == model.IdentityCard))
                {
                    ModelState.AddModelError("IdentityCard", "CMND/CCCD đã tồn tại.");
                    return View(model);
                }
                var employee = new Employee
                {
                    UserId = model.UserId,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Address = model.Address,
                    Position = model.Position,
                    IdentityCard = model.IdentityCard,
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    IsActive = true
                };

                try
                {
                    _context.Employees.Add(employee);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm thông tin Employee thành công!";
                    return RedirectToAction("Manage", "Account");

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi khi thêm thông tin: {ex.Message}");
                    return View(model);
                }
            }
            else
            {
                // Log lỗi ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                System.Diagnostics.Debug.WriteLine("ModelState Errors: " + string.Join(", ", errors));
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult ToggleEmployeeStatus(int userId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee != null)
            {
                employee.IsActive = !employee.IsActive; // Đảo ngược trạng thái
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên!";
            }
            return RedirectToAction("Manage","Account");
        }
        [HttpGet]
        public IActionResult ManageEmployees(string searchString, string positionFilter, bool? isActiveFilter)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền truy cập.";
                return RedirectToAction("Login", "Account");
            }

            var employees = from e in _context.Employees
                            join u in _context.Users on e.UserId equals u.UserId into userGroup // Sử dụng vào biến nhóm
                            from u in userGroup.DefaultIfEmpty() // Left join
                            select new EmployeeViewModel
                            {
                                EmployeeId = e.EmployeeId,
                                UserId = e.UserId.Value, // Giữ nguyên, có thể null
                                Username = u != null ? u.Username : null, // Nếu không có tài khoản, gán null
                                FullName = e.FullName,
                                PhoneNumber = e.PhoneNumber,
                                DateOfBirth = e.DateOfBirth,
                                Gender = e.Gender,
                                Address = e.Address,
                                Position = e.Position,
                                IdentityCard = e.IdentityCard,
                                StartDate = e.StartDate,
                                IsActive = e.IsActive
                            };

            // Tìm kiếm theo tên hoặc số điện thoại
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.FullName.Contains(searchString) || e.PhoneNumber.Contains(searchString));
            }

            // Lọc theo chức vụ
            if (!string.IsNullOrEmpty(positionFilter))
            {
                employees = employees.Where(e => e.Position == positionFilter);
            }

            // Lọc theo trạng thái hoạt động
            if (isActiveFilter.HasValue)
            {
                employees = employees.Where(e => e.IsActive == isActiveFilter.Value);
            }

            return View(employees.ToList());
        }


        // GET: /Admin/CreateEmployee (Tạo mới nhân viên)
        public IActionResult CreateEmployee()
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền thêm nhân viên.";
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: /Admin/CreateEmployee
        [HttpPost]
        public IActionResult CreateEmployee(CreateEmployeeViewModel model)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền thêm nhân viên.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra căn cước công dân có bị trùng không
                if (_context.Employees.Any(e => e.IdentityCard == model.IdentityCard))
                {
                    ModelState.AddModelError("IdentityCard", "Số căn cước công dân đã tồn tại.");
                    return View(model);
                }
                if (_context.Employees.Any(e => e.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("", "Số điện thoại đã tồn tại.");
                    return View(model);
                }
                try
                {
                    // Thêm nhân viên vào bảng Employees
                    var employee = new Employee
                    {
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        Address = model.Address,
                        Position = model.Position,
                        IdentityCard = model.IdentityCard,
                        StartDate = DateOnly.FromDateTime(DateTime.Now),
                        IsActive = true
                    };

                    _context.Employees.Add(employee);
                    _context.SaveChanges();
                   
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction("ManageEmployees");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi khi thêm nhân viên: {ex.Message}");
                    return View(model);
                }
            }

            return View(model);
        }


        // GET: /Admin/EditEmployee/5 (Sửa nhân viên)
        public IActionResult EditEmployee(int id)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền chỉnh sửa nhân viên.";
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                return RedirectToAction("ManageEmployees");
            }
            var username = employee.UserId.HasValue
       ? _context.Users.Where(u => u.UserId == employee.UserId).Select(u => u.Username).FirstOrDefault()
       : null;
            var model = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                UserId = employee.UserId.Value,
                Username = username,
                FullName = employee.FullName,
                PhoneNumber = employee.PhoneNumber,
                DateOfBirth = employee.DateOfBirth,
                Gender = employee.Gender,
                Address = employee.Address,
                Position = employee.Position,
                IdentityCard = employee.IdentityCard,
                StartDate = employee.StartDate,
                IsActive = employee.IsActive
            };

            return View(model);
        }

        // POST: /Admin/EditEmployee
        [HttpPost]
        public IActionResult EditEmployee(EmployeeViewModel model)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền chỉnh sửa nhân viên.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var employee = _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                    return RedirectToAction("ManageEmployees");
                }

                try
                {
                    employee.FullName = model.FullName;
                    employee.PhoneNumber = model.PhoneNumber;
                    employee.DateOfBirth = model.DateOfBirth;
                    employee.Gender = model.Gender;
                    employee.Address = model.Address;
                    employee.Position = model.Position;
                    employee.IdentityCard = model.IdentityCard;
                    employee.IsActive = model.IsActive;

                    _context.Employees.Update(employee);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                    return RedirectToAction("ManageEmployees");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi khi cập nhật nhân viên: {ex.Message}");
                    return View(model);
                }
            }
            return View(model);
        }

        // POST: /Admin/DeleteEmployee/5 (Xóa nhân viên)
        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền xóa nhân viên.";
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                return RedirectToAction("ManageEmployees");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == employee.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản liên quan không tồn tại.";
                return RedirectToAction("ManageEmployees");
            }

            try
            {
                _context.Employees.Remove(employee);
                _context.Users.Remove(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi xóa nhân viên: {ex.Message}";
            }

            return RedirectToAction("ManageEmployees");
        }
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
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
