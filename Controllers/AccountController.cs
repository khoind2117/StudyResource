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
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
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
                var userByUsername = await _userManager.FindByNameAsync(registerViewModel.Email);
                if (userByUsername != null)
                {
                    ModelState.AddModelError("UserName", "Tài khoản Email đã được sử dụng, vui lòng chọn tài khoản Email khác!");
                    return View(registerViewModel);
                }

                var user = new User
                {
                    UserName = registerViewModel.Email,
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
        public IActionResult Manage()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var model = new ManageViewModel
            {
                Profile = new ProfileViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber
                },
                ChangePassword = new ChangePasswordViewModel()
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ManageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                user.Email = model.Profile.Email;
                user.PhoneNumber = model.Profile.PhoneNumber;
                user.FirstName = model.Profile.FirstName;
                user.LastName = model.Profile.LastName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["ProfileUpdateMessage"] = "Successfully updated.";
                    return RedirectToAction(nameof(Manage));
                }
                else
                {
                    // Ghi lại thông tin lỗi
                    _logger.LogError("Error updating user profile: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
            }

            return View("Manage", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ManageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.ChangePassword.OldPassword, model.ChangePassword.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _signInManager.RefreshSignInAsync(user);
            }
            return View("Manage", model);
        }

    
/*
            [HttpPost]
            public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View("Manage", new ManageViewModel { ChangePassword = model });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                if (!string.IsNullOrEmpty(model.OldPassword) &&
                    !string.IsNullOrEmpty(model.NewPassword) &&
                    !string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    if (model.NewPassword != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ChangePassword.ConfirmPassword", "Xác nhận mật khẩu không khớp.");
                        return View("Manage", new ManageViewModel { ChangePassword = model });
                    }

                    var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, model.OldPassword);
                    if (!isPasswordCorrect)
                    {
                        ModelState.AddModelError("ChangePassword.OldPassword", "Mật khẩu hiện tại không đúng.");
                        return View("Manage", new ManageViewModel { ChangePassword = model });
                    }

                    var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (!passwordChangeResult.Succeeded)
                    {
                        foreach (var error in passwordChangeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("Manage", new ManageViewModel { ChangePassword = model });
                    }
                }

                TempData["PasswordChangeMessage"] = "Password changed successfully.";
                return RedirectToAction(nameof(Manage));
            }*/

    }
}
