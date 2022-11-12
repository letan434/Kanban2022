using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using KanbanApp.BackendServer.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace KanbanApp.BackendServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public async Task<IActionResult> OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            //var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            //var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            //return new ChallengeResult(provider, properties);
            // Kiểm tra yêu cầu dịch vụ provider tồn tại
            var listprovider = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var provider_process = listprovider.Find((m) => m.Name == provider);
            if (provider_process == null)
            {
                return NotFound("Dịch vụ không chính xác: " + provider);
            }

            // redirectUrl - là Url sẽ chuyển hướng đến - sau khi CallbackPath (/dang-nhap-tu-google) thi hành xong
            // nó bằng identity/account/externallogin?handler=Callback 
            // tức là gọi OnGetCallbackAsync 
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });

            // Cấu hình 
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Chuyển hướng đến dịch vụ ngoài (Googe, Facebook)
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                var userExisted = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (userExisted != null)
                {
                    // Đã có Acount, đã liên kết với tài khoản ngoài - nhưng không đăng nhập được
                    // có thể do chưa kích hoạt email
                    return RedirectToPage("./RegisterConfirmation", new { Email = userExisted.Email });

                }

                // Chưa có Account liên kết với tài khoản ngoài
                // Hiện thị form để thực hiện bước tiếp theo ở OnPostConfirmationAsync

                ReturnUrl = returnUrl;
                LoginProvider = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    // Có thông tin về email từ info, lấy email này hiện thị ở Form
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }

                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Lấy lại Info
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                string externalMail = null;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    externalMail = info.Principal.FindFirstValue(ClaimTypes.Email);
                }
                var userWithexternalMail = (externalMail != null) ? (await _userManager.FindByEmailAsync(externalMail)) : null;

                // Xử lý khi có thông tin về email từ info, đồng thời có user với email đó
                // trường hợp này sẽ thực hiện liên kết tài khoản ngoài + xác thực email luôn     
                if ((userWithexternalMail != null) && (Input.Email == externalMail))
                {
                    // xác nhận email luôn nếu chưa xác nhận
                    if (!userWithexternalMail.EmailConfirmed)
                    {
                        var codeactive = await _userManager.GenerateEmailConfirmationTokenAsync(userWithexternalMail);
                        await _userManager.ConfirmEmailAsync(userWithexternalMail, codeactive);
                    }
                    // Thực hiện liên kết info và user
                    var resultAdd = await _userManager.AddLoginAsync(userWithexternalMail, info);
                    if (resultAdd.Succeeded)
                    {
                        // Thực hiện login    
                        await _signInManager.SignInAsync(userWithexternalMail, isPersistent: false);
                       
                    }
                   
                }
                // Tài khoản chưa có, tạo tài khoản mới
                var user = new User { UserName = Input.Email, Email = Input.Email };
                user.FirstName = info.Principal.Identity.Name;
                user.LastName = info.Principal.Identity.Name;
                user.Dob = DateTime.Now;
                user.Email = Input.Email;
                user.EmailConfirmed = true;
                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {

                    // Liên kết tài khoản ngoài với tài khoản vừa tạo
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Đã tạo user mới từ thông tin {Name}.", info.LoginProvider);
                        // Email tạo tài khoản và email từ info giống nhau -> xác thực email luôn
                        //if (user.Email == externalMail)
                        //{                            
                        //    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        //    return ViewComponent(MessagePage.COMPONENTNAME, new MessagePage.Message()
                        //    {
                        //        title = "TẠO VÀ LIÊN KẾT TÀI KHOẢN",
                        //        urlredirect = returnUrl,
                        //        htmlcontent = $"Đã tạo và liên kết tài khoản, kích hoạt email thành công"
                        //    });
                        //}

                        // Trường hợp này Email tạo User khác với Email từ info (hoặc info không có email)
                        // sẽ gửi email xác để người dùng xác thực rồi mới có thể đăng nhập
                       

                        // Đăng nhập ngay do không yêu cầu xác nhận email
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            LoginProvider = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
