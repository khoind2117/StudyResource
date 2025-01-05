using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.ViewModels.Account;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Web.Helpers;

namespace StudyResource.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            LoginViewModel loginViewModel = new LoginViewModel();
            loginViewModel.ReturnUrl = returnUrl ?? Url.Content("~/");
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginViewModel.UserName);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, loginViewModel.Password, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            // Chuyển hướng đến trang quản trị
                            return RedirectToAction("Index", "Home", new { Area = "Admin" });
                        }


                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email hoặc mật khẩu không đúng. Vui lòng thử lại");
                        return View(loginViewModel);
                    }
                }
            }

            ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin đăng nhập");
            return View(loginViewModel);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            RegisterViewModel registerViewModel = new RegisterViewModel();
            registerViewModel.ReturnUrl = returnUrl;
            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var userByUsername = await _userManager.FindByNameAsync(registerViewModel.Username);
                if (userByUsername != null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã được sử dụng, vui lòng chọn tên đăng nhập khác!");
                    return View(registerViewModel);
                }

                var user = new User
                {
                    UserName = registerViewModel.Username,
                    FirstName = registerViewModel.FirstName,
                    LastName = registerViewModel.LastName,
                    CreatedAt = DateTime.Now,
                    PhoneNumber = registerViewModel.PhoneNumber,
                    Email = registerViewModel.Email,
                };

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);
                if (result.Succeeded)
                {
                    #region Role
                    await _userManager.AddToRoleAsync(user, "User"); // Gán role Admin/User
                    #endregion
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("Password", "Tạo tài khoản không thành công");
                }
            }
            return View(registerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Manage()
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ManageViewModel
            {
                Manage = new ProfileViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber
                },
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Manage(string userName, string email, string firstName, string lastName, string phoneNumber)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Error");
            }

            user.UserName = userName;
            user.Email = email;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction("Manage");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(new ManageViewModel()); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra mật khẩu hiện tại
            var result = await _userManager.CheckPasswordAsync(user, oldPassword);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                return View("Manage", new ManageViewModel());
            }

            // Thay đổi mật khẩu
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Manage", new ManageViewModel());
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> ValidateCurrentPassword(string currentPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { isValid = false, message = "Người dùng không tồn tại." });
            }

            var isValid = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!isValid)
            {
                return Json(new { isValid = false, message = "Mật khẩu hiện tại không đúng." });
            }

            return Json(new { isValid = true });
        }


    }
}
