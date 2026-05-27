using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Event_Management_System.Helpers
{
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var adminId = context.HttpContext.Session.GetInt32("AdminID");
            
            if (!adminId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
        }
    }
}
