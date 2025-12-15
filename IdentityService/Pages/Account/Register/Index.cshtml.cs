using Duende.IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityService.Pages.Account.Register
{


    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        public UserManager<ApplicationUser> _userManager;

        public Index(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public RegisterViewModel input { get; set; }

        [BindProperty]
        public bool RegisterSuccess { get; set; }

        public IActionResult OnGet(string returnUrl)
        {
            input = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (input?.Button != "register") return Redirect("~/");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = input.Username,
                    Email = input.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, input.Password!);

                if (result.Succeeded)
                {
                    await _userManager.AddClaimsAsync(user, [
                        new Claim(JwtClaimTypes.Name, input.FullName!),
                    ]);

                    RegisterSuccess = true;
                }
            }

            return Page();
        }
    }
}
