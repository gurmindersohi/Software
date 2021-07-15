using System;
using Stripe;
using Sohi.Models;
using System.Threading.Tasks;

namespace Sohi.Web.Services.Billing
{
    public interface IBillingService
    {
        Task<Sohi.Models.Account> UpdateBillingDetails(Sohi.Models.Account updatedAccount);

    }
}
