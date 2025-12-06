using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly DataContext _dataContext;

        public AccountController(UserManager<AppUserModel> userManage,
            SignInManager<AppUserModel> signInManager,
            DataContext context)
        {
            _dataContext = context;
            _userManage = userManage;
            _signInManager = signInManager;
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        public async Task<IActionResult> UpdateAccount()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userById = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            else
            {
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);
                userById.PasswordHash = passwordHash;

                _dataContext.Update(userById);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Account Information Successfully";
            }
            return RedirectToAction("UpdateAccount", "Account");
        }

        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email && u.Token == user.Token)
                .FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email && u.Token == user.Token)
                .FirstOrDefaultAsync();

            if (checkuser != null)
            {
                string newtoken = Guid.NewGuid().ToString();
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManage.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgetPass", "Account");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgetPass", "Account");
            }

            // Update token
            string token = Guid.NewGuid().ToString();
            checkMail.Token = token;
            _dataContext.Update(checkMail);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "A token has been generated. Implement sending mail separately if needed.";
            return RedirectToAction("ForgetPass", "Account");
        }

        public IActionResult ForgetPass(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                   // TempData["success"] = "Đăng nhập thành công";
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            }
            return View(loginVM);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var Orders = await _dataContext.Orders
               .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();

            ViewBag.UserEmail = userEmail;
            return View(Orders);
        }

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while canceling the order.");
            }

            return RedirectToAction("History", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
                var result = await _userManage.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    var roleAssignResult = await _userManage.AddToRoleAsync(newUser, "Admin");

                    if (!roleAssignResult.Succeeded)
                    {
                        foreach (var error in roleAssignResult.Errors)
                            ModelState.AddModelError("", error.Description);
                        return View(user);
                    }

                   // TempData["success"] = "Tạo khách hàng thành công và đã gán quyền Admin.";
                    return Redirect("/account/login");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(user);
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await HttpContext.SignOutAsync();
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            });
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string UserName = email.Split('@')[0];

            var existingUser = await _userManage.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "Ta!123");

                var newUser = new AppUserModel { UserName = UserName, Email = email, PasswordHash = hashedPassword };
                var createUserResult = await _userManage.CreateAsync(newUser);

                if (!createUserResult.Succeeded)
                {
                    TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                    return RedirectToAction("Login", "Account");
                }
                var roleExist = await _userManage.IsInRoleAsync(newUser, "Admin");
                if (!roleExist)
                {
                    await _userManage.AddToRoleAsync(newUser, "Admin");
                }

                await _signInManager.SignInAsync(newUser, isPersistent: false);
                TempData["success"] = "Đăng ký tài khoản thành công.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
