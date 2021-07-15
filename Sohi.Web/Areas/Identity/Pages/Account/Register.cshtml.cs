using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Accounts;

namespace Sohi.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    //[Authorize(Roles = "Gurminder")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;


        private readonly IEmailSender _emailSender;


        //public bool IsPlanSelected { get; set; } = true;


        private IConfiguration _config;

        private readonly IAccountService _accountService;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IAccountService accountService,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _accountService = accountService;

            _roleManager = roleManager;
            _config = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public Plan Plans { get; set; } = new Plan();

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "First Name is required.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last Name is required.")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required.")]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, ErrorMessage = "Password needs to be at least 6 characters.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            //public bool TermsOfService { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            //Plans.Id = "101";
            //Plans.Price = "101";
            //Plans.Type = "New";
            //Plans.BillingPeriod = "Monthly";
            //Plans.Total = "0.00";

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {

                Sohi.Models.Account newAccount = MapAccountValues();
                newAccount.Email = Input.Email;

                Sohi.Models.Account account = await _accountService.CreateAccount(newAccount);

                var user = new User
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    PhoneNumber = Input.PhoneNumber,
                    AccountId = account.AccountId.ToString()
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var role = await _roleManager.FindByIdAsync("cd9a0163-fde3-4bc1-a9b6-1c76926e78ff");

                    var assigned = await _userManager.AddToRoleAsync(user, role.Name);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");


                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }


        private Sohi.Models.Account MapAccountValues()
        {
            Sohi.Models.Account account = new Sohi.Models.Account();
            account.AccountId = Guid.NewGuid();
            account.AccountType = "Diamond";
            account.Email = "";
            account.UsersLimit = "10";

            account.TrialExpiry = DateTime.Now.AddDays(14);
            account.IsAccountPaid = false;
            account.IsDeleted = false;
            account.OnHold = false;

            account.CreatedBy = "Home";
            account.CreatedOn = DateTime.Now;
            account.ModifiedBy = "Home";
            account.ModifiedOn = DateTime.Now;
            account.IsActive = true;

            return account;
        }


    }
}

