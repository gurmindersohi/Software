using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using Stripe.Checkout;

namespace Sohi.Web.Pages.Website
{
    [AllowAnonymous]
    public class PricingModel : PageModel
    {
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
           var result = await RedirectToStripe();

            return result;
        }

        private async Task<IActionResult> RedirectToStripe() {

            StripeConfiguration.ApiKey = "sk_test_51J0afCCAgHgewQ1l86OqtHa9Au325LaR56Hj6MMInQEYjTSGNgaXBtjs8UvzuSLjpl3J5UxvomecnZpz8ljw0AOo00bhcWOxGT";

            var priceId = "price_1JBOINCAgHgewQ1lVCPssJXG";

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
            else {

                string url = "/identity/account/login?returnurl=" + session.Url;

                return LocalRedirect(url);
                //return Redirect(session.Url);
            }

        }

    }
}
