using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace Sohi.Web.Pages.Website
{
    [AllowAnonymous]
    public class PricingModel : PageModel
    {

        private IConfiguration _config;

        public PricingModel(IConfiguration configuration)
        {
            _config = configuration;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var Key = _config.GetSection("Stripe").GetSection("Key").Value;

            var price = "price_1JBOINCAgHgewQ1lVCPssJXG";

            var result = await RedirectToStripe(Key, price);

            return result;
        }

        public async Task<IActionResult> OnPostBasicPlan()
        {
            var Key = _config.GetSection("Stripe").GetSection("Key").Value;

            var price = "price_1JDWNPCAgHgewQ1lA03AvdDH";

            var result = await RedirectToStripe(Key, price);

            return result;
        }

        public async Task<IActionResult> OnPostUnlimitedPlan()
        {
            var Key = _config.GetSection("Stripe").GetSection("Key").Value;

            var price = "price_1JDYooCAgHgewQ1l2HMmFMBd";

            var result = await RedirectToStripe(Key, price);

            return result;
        }

        private async Task<IActionResult> RedirectToStripe(string Key, string priceid)
        {

            StripeConfiguration.ApiKey = Key;

            var priceId = priceid;

            var options = new SessionCreateOptions
            {
                //AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },

                PaymentMethodTypes = new List<string>
                {
                     "card",
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    DefaultTaxRates = new List<string>
                    {
                        "txr_1JCpVoCAgHgewQ1ljmJ5oDvb"
                    },
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },


                Mode = "subscription",
                SuccessUrl = "https://localhost:5001/Website/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:5001/Website/Pricing",
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            //return session;

            if (User.Identity.IsAuthenticated)
            {

                return Redirect(session.Url);
            }
            else
            {

                string url = "/identity/account/login?returnurl=" + session.Url;

                return LocalRedirect(url);
                //return Redirect(session.Url);
            }

        }

    }
}
