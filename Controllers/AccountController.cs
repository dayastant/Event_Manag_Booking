using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;
using Event_Management_System.Models.ViewModels;
using Event_Management_System.Helpers;
using System.Runtime.Versioning;

namespace Event_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email == model.Email);

                if (member != null && AuthHelper.VerifyPassword(model.Password, member.PasswordHash))
                {
                    if (member.Status != "Active")
                    {
                        ModelState.AddModelError(string.Empty, "Your account is not active.");
                        return View(model);
                    }

                    // Set session
                    HttpContext.Session.SetInt32("MemberID", member.MemberID);
                    HttpContext.Session.SetString("MemberName", member.FirstName);
                    HttpContext.Session.SetString("MemberEmail", member.Email);
                    HttpContext.Session.SetString("ProfilePhotoUrl", member.ProfilePhotoBase64 ?? "/images/prf.png");
                    
                    // Handle Remember Me
                    if (model.RememberMe)
                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(30),
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax
                        };
                        Response.Cookies.Append("RememberMe", member.MemberID.ToString(), cookieOptions);
                    }
                    
                    // Ensure session is saved before redirect
                    await HttpContext.Session.CommitAsync();

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email == model.Email);

                if (existingMember != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                // Handle profile picture
                byte[]? profilePhotoBytes = null;
                string? profilePhotoContentType = null;
                
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    // User uploaded a profile picture - convert to bytes
                    string dataUrl = await ImageHelper.ConvertToBase64(model.ProfilePicture);
                    var (bytes, contentType) = ImageHelper.ConvertDataUrlToBytes(dataUrl);
                    profilePhotoBytes = bytes;
                    profilePhotoContentType = contentType;
                }
                else
                {
                    // Generate initials-based profile picture
                    string initialsDataUrl = ImageHelper.GenerateInitialsImage(model.FirstName, model.LastName);
                    var (bytes, contentType) = ImageHelper.ConvertDataUrlToBytes(initialsDataUrl);
                    profilePhotoBytes = bytes;
                    profilePhotoContentType = contentType;
                }

                // Create new member
                var member = new Member
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = AuthHelper.HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    PreferredCategory = model.PreferredCategory,
                    ProfilePhoto = profilePhotoBytes,
                    ProfilePhotoContentType = profilePhotoContentType,
                    RegistrationDate = DateTime.Now,
                    Status = "Active"
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                // Auto login after registration
                HttpContext.Session.SetInt32("MemberID", member.MemberID);
                HttpContext.Session.SetString("MemberName", member.FirstName);
                HttpContext.Session.SetString("MemberEmail", member.Email);
                HttpContext.Session.SetString("ProfilePhotoUrl", member.ProfilePhotoBase64 ?? "/images/prf.png");
                
                // Ensure session is saved before redirect
                await HttpContext.Session.CommitAsync();

                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        // GET/POST: Account/Logout
        [HttpGet]
        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult Logout()
        {
            // Clear all session data and abandon session
            HttpContext.Session.Clear();
            
            // Remove Remember Me cookie with proper options
            if (Request.Cookies["RememberMe"] != null)
            {
                Response.Cookies.Delete("RememberMe", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }
            
            // Add comprehensive cache control headers to prevent back button access
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            // Redirect to login page with cache-busting parameter
            return RedirectToAction("Login", "Account", new { logout = DateTime.UtcNow.Ticks });
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email == model.Email);

                if (member != null)
                {
                    // Generate password reset token
                    var token = Guid.NewGuid().ToString();
                    var tokenExpiry = DateTime.Now.AddHours(1);

                    // Store token in session for demo purposes (in production, use database)
                    HttpContext.Session.SetString($"PasswordResetToken_{model.Email}", token);
                    HttpContext.Session.SetString($"PasswordResetExpiry_{model.Email}", tokenExpiry.ToString());

                    // MOCK EMAIL SERVICE - In production, send email with reset link
                    var resetLink = Url.Action("ResetPassword", "Account", 
                        new { token = token, email = model.Email }, Request.Scheme);
                    
                    // Show success message on same page to avoid TempData issues
                    ViewBag.Success = $"Password reset instructions have been sent to {model.Email}.";
                    ViewBag.ResetLink = resetLink; // For demo purposes
                }
                else
                {
                    // Don't reveal that email doesn't exist for security
                    ViewBag.Success = $"If an account with {model.Email} exists, password reset instructions have been sent.";
                }

                return View(model);
            }

            return View(model);
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate token
                var storedToken = HttpContext.Session.GetString($"PasswordResetToken_{model.Email}");
                var expiryString = HttpContext.Session.GetString($"PasswordResetExpiry_{model.Email}");

                if (string.IsNullOrEmpty(storedToken) || storedToken != model.Token)
                {
                    ModelState.AddModelError(string.Empty, "Invalid or expired password reset token.");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(expiryString) && DateTime.TryParse(expiryString, out DateTime expiry))
                {
                    if (DateTime.Now > expiry)
                    {
                        ModelState.AddModelError(string.Empty, "Password reset token has expired.");
                        HttpContext.Session.Remove($"PasswordResetToken_{model.Email}");
                        HttpContext.Session.Remove($"PasswordResetExpiry_{model.Email}");
                        return View(model);
                    }
                }

                // Find member and update password
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email == model.Email);

                if (member != null)
                {
                    member.PasswordHash = AuthHelper.HashPassword(model.NewPassword);
                    await _context.SaveChangesAsync();

                    // Clear token from session
                    HttpContext.Session.Remove($"PasswordResetToken_{model.Email}");
                    HttpContext.Session.Remove($"PasswordResetExpiry_{model.Email}");

                    TempData["Success"] = "Password has been reset successfully. Please login with your new password.";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError(string.Empty, "User not found.");
            }

            return View(model);
        }

        // API Endpoint for Email Validation
        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { exists = false });
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email == email);
                
            return Json(new { exists = member != null });
        }
    }
}
