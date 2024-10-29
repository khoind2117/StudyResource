using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudyResource.Data;
using StudyResource.Models;
using StudyResource.ViewModels.Account;
using System.Text;

namespace StudyResource.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(ApplicationDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
    }
}
