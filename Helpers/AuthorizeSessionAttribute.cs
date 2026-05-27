using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Event_Management_System.Helpers
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var memberId = context.HttpContext.Session.GetInt32("MemberID");
            
            if (memberId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            }
            
            base.OnActionExecuting(context);
        }
    }
}
