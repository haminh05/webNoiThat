using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cnpm.Helpers
{
    public class AuthHelper : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthHelper(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            // Kiểm tra Session Role có khớp với roles được yêu cầu?
            if (string.IsNullOrEmpty(role) || !_roles.Contains(role))
            {
                // Chuyển hướng trang AccessDenied
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
