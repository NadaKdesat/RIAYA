    using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using RIAYA.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace RIAYA.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDbContext _context;

        public UserController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user); 
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(user);
            }
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.UserType = "user";
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        //[HttpGet]
        //public IActionResult LoginWithGoogle()
        //{
        //    var redirectUrl = Url.Action("GoogleResponse", "User");
        //    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        //    return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        //}

        //[HttpGet]
        //public async Task<IActionResult> GoogleResponse()
        //{
        //    var result = await HttpContext.AuthenticateAsync("External");

        //    if (result?.Principal == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        //    var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;


        //    return RedirectToAction("Index", "Home");
        //}

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(e => e.Email == Email);

                if (user != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
                    {
                        if (rememberMe)
                        {
                            CookieOptions options = new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(7), 
                                IsEssential = true,
                                HttpOnly = true,
                                Secure = true
                            };
                            Response.Cookies.Append("IsLoggedIn", "true", options);
                            Response.Cookies.Append("UserId", user.Id.ToString(), options);
                            Response.Cookies.Append("FullName", user.FullName ?? "", options);
                            Response.Cookies.Append("UserEmail", user.Email ?? "", options);
                            Response.Cookies.Append("Phone", user.Phone ?? "", options);
                            Response.Cookies.Append("City", user.City ?? "", options);
                            Response.Cookies.Append("UserType", user.UserType ?? "", options);
                        }

                        HttpContext.Session.SetString("IsLoggedIn", "true");
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("FullName", user.FullName ?? "");
                        HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                        HttpContext.Session.SetString("Phone", user.Phone ?? "");
                        HttpContext.Session.SetString("City", user.City ?? "");
                        HttpContext.Session.SetString("UserType", user.UserType);

                        switch (user.UserType.ToLower())
                        {
                            case "admin":
                                return RedirectToAction("AdminDashboard", "Admin");
                            case "provider":
                                return RedirectToAction("ProviderDashboard", "Provider");
                            case "user":
                                return RedirectToAction("Index", "Home");
                            default:
                                ViewBag.Message = "Invalid user type.";
                                return View();
                        }
                    }
                    else
                    {
                        ViewBag.Message = "The password you entered is incorrect.";
                    }
                }
                else
                {
                    ViewBag.Message = "The email address you entered is not registered.";
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Please enter your email address.";
                return View();
            }

            var role = HttpContext.Session.GetString("UserRole");
            object? user = FindUserByEmail(email);

            if (user == null)
            {
                ViewBag.Error = "No account found with this email.";
                return View();
            }

            string resetToken = Guid.NewGuid().ToString();
            string resetLink = Url.Action("ResetPassword", "User", new { token = resetToken, email = email }, Request.Scheme);

            if (!SendResetEmail(email, resetLink))
            {
                ViewBag.Error = "An error occurred while sending the email.";
                return View();
            }

            ViewBag.Message = "A password reset link has been sent to your email.";
            return View();
        }

        private bool SendResetEmail(string email, string resetLink)
        {
            try
            {
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("nadaqdesat@gmail.com"),
                    Subject = "Password Reset Request",
                    Body = $"Click the following link to reset your password: <a href='{resetLink}'>Reset Password</a>",
                    IsBodyHtml = true
                };

                mail.To.Add(email);

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("nadaqdesat@gmail.com", "aibl avfx vyag dxgn"),
                    EnableSsl = true
                };

                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid password reset link.");
            }

            object? user = FindUserByEmail(email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string token, string newPassword)
        {
            object? user = FindUserByEmail(email);

            if (user == null)
            {
                ViewBag.Error = "Invalid request.";
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            if (user is User userObj)
            {
                userObj.PasswordHash = hashedPassword;
            }
            else
            {
                ViewBag.Error = "Invalid user type.";
                return View();
            }
            _context.SaveChanges();

            ViewBag.Message = "Password has been successfully reset.";
            return View();
        }



        private object? FindUserByEmail(string email)
        {
            return (object?)_context.Users.FirstOrDefault(e => e.Email == email);
        }


        public IActionResult Profile()
        {
            string fullName = HttpContext.Session.GetString("FullName")
                              ?? Request.Cookies["FullName"]
                              ?? "";

            string email = HttpContext.Session.GetString("UserEmail")
                           ?? Request.Cookies["UserEmail"]
                           ?? "";

            string phone = HttpContext.Session.GetString("Phone")
                           ?? Request.Cookies["Phone"]
                           ?? "";

            string city = HttpContext.Session.GetString("City")
                          ?? Request.Cookies["City"]
                          ?? "";

            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Phone = phone;
            ViewBag.City = city;

            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string FullName, string Email, string Phone, string City)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail")
                           ?? Request.Cookies["UserEmail"]
                           ?? "";
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user != null)
            {
                user.FullName = FullName;
                user.Email = Email;
                user.Phone = Phone;
                user.City = City;
                _context.SaveChanges();

                HttpContext.Session.SetString("FullName", FullName);
                HttpContext.Session.SetString("Phone", Phone);
                HttpContext.Session.SetString("City", City);

                CookieOptions option = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true
                };
                Response.Cookies.Append("FullName", FullName, option);
                Response.Cookies.Append("Phone", Phone, option);
                Response.Cookies.Append("City", City, option);
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail")
                           ?? Request.Cookies["UserEmail"]
                           ?? "";
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user != null && BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
            {
                if (NewPassword == ConfirmPassword)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                    _context.SaveChanges();

                    // Return success response as JSON
                    ViewBag.successMessage = "Password changed successfully!";
                    return Json(new { success = true, message = "Password changed successfully!" });
                }
                else
                {
                    ViewBag.errorMessage= "The new passwords do not match.";
                    // Return error response as JSON
                    return Json(new { success = false, message = "The new passwords do not match." });
                }
            }
            else
            {
                ViewBag.errorMessage = "Current password is incorrect.";
                // Return error response as JSON
                return Json(new { success = false, message = "Current password is incorrect." });
            }
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("IsLoggedIn");
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("FullName");
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("Phone");
            Response.Cookies.Delete("City");
            Response.Cookies.Delete("UserType");

            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }


    }
}
