using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Sohi.Web.Models;
using Sohi.Web.Services.Accounts;
using Sohi.Web.Services.Billing;
using Stripe;
using Stripe.Checkout;

namespace Sohi.Web.Pages.Website
{
    public class SuccessModel : PageModel
    {

        public string CustomerName { get; set; }

        private readonly IBillingService _billingService;
        private readonly IAccountService _accountService;

        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private IConfiguration _config;

        private string AccountId { get; set; }

        public SuccessModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IBillingService billingService,
            IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;

            _config = configuration;

            _billingService = billingService;

            _accountService = accountService;
        }

        public async Task<ActionResult> OnGet(string session_id)
        {

            var user = await _userManager.GetUserAsync(User);

            AccountId = user.AccountId;

            //Optional Key! Remove when Final Deployment.
            //StripeConfiguration.ApiKey = "sk_test_51J0afCCAgHgewQ1l86OqtHa9Au325LaR56Hj6MMInQEYjTSGNgaXBtjs8UvzuSLjpl3J5UxvomecnZpz8ljw0AOo00bhcWOxGT";

            //session_id = "cs_test_a10JO1PRpSkTRQn4v8GgJiRDBEKQzzPvYnrFCjOkvmh6xwmnfpVtRtxxju";

            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id);

            var customerService = new CustomerService();
            Customer customer = customerService.Get(session.CustomerId);

            var service = new SubscriptionService();
            var subscription = service.Get(session.SubscriptionId);


            var productId = subscription.Items.Data[0].Plan.ProductId;

            var productService = new ProductService();
            var product = productService.Get(productId);

            var updatedAccount = new Sohi.Models.Account();

            updatedAccount.AccountId = Guid.Parse(AccountId);
            updatedAccount.CustomerId = session.CustomerId;
            updatedAccount.SubscriptionId = session.SubscriptionId;

            updatedAccount.AccountType = product.Name;

            try
            {
                await UpdateBillingDetails(updatedAccount);
            }
            catch (Exception ex)
            {

            }

            CustomerName = customer.Name;


            //return Content($"<html><body><h1>Thanks for your order, {customer.Name}!</h1></body></html>");

            return Page();

        }

        private async Task UpdateBillingDetails(Sohi.Models.Account updatedAccount)
        {
            var result = await _billingService.UpdateBillingDetails(updatedAccount);
        }



    }
}
