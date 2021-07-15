using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sohi.Api.Models.Billing;
using Sohi.Models;

namespace Sohi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private readonly IBillingRepository _billingRepository;

        public BillingController(IBillingRepository billingRepository)
        {
            _billingRepository = billingRepository;
        }

        [HttpPut]
        public async Task<ActionResult<Account>> UpdateBillingDetails(Account account)
        {
            try
            {
                var accountToUpdate = await _billingRepository.GetAccount(account.AccountId);

                if (accountToUpdate == null)
                    return NotFound($"Account with Id = {account.AccountId} not found");

                return await _billingRepository.UpdateAccount(account);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }



    }
}
