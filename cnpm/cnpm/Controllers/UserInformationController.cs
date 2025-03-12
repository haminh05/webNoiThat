using cnpm.Models;
using cnpm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cnpm.Controllers
{
    [Authorize] //yêu cầu đăng nhập
    public class UserInformationController : Controller
    {
        private readonly BiaContext _context;

        public UserInformationController(BiaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserInformationViewModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var userInfo = new UserInformation
            {
                UserId = userId,
                FullName = model.FullName,
                ShippingAddress = model.ShippingAddress,
                PhoneNumber = model.PhoneNumber
            };

            _context.UserInformations.Add(userInfo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Orders");
        }
    }

}
