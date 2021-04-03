using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcWithExternalLogin.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MvcWithExternalLogin.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel model = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl=null)//if we have only one provide we can hardcode the provider value
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var redirecturl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirecturl);
            return Challenge(properties,provider);//this method is used for external login providers
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel model = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            if (remoteError is not null)
            {
                ModelState.AddModelError(string.Empty,$"Error from External Provider {remoteError}");
                return View("Login",model);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                ModelState.AddModelError(string.Empty, "Error loading External login information");
                return View("Login", model);
            }
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor:true);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                //we have to use name instead of email because if user's email is private on github we wont get their email
                var name= info.Principal.FindFirstValue(ClaimTypes.Name);
                if (name is not null)
                {
                    var user = await _userManager.FindByNameAsync(name);
                    if (user is null)
                    {
                        user = new IdentityUser()
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Name)
                        };
                        await _userManager.CreateAsync(user); 
                    }

                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent:false);
                    return LocalRedirect(returnUrl);
                }
                return View("Login");

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            return LocalRedirect("~/");
        }
    }
}
