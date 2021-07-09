using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sohi.Api.Models.Accounts;
using Sohi.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sohi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpGet("{accountid:Guid}")]
        public async Task<ActionResult<Account>> GetAccount(Guid accountid)
        {
            try
            {
                var result = await _accountRepository.GetAccount(accountid);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPut]
        public async Task<ActionResult<Account>> UpdateAccount(Account account)
        {
            try
            {
                var accountToUpdate = await _accountRepository.GetAccount(account.AccountId);

                if (accountToUpdate == null)
                    return NotFound($"Account with Id = {account.AccountId} not found");

                return await _accountRepository.UpdateAccount(account);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount(Account newAccount)
        {
            try
            {
                if (newAccount == null)
                    return BadRequest();

                var existingAccount = await _accountRepository.GetAccountByEmail(newAccount.Email);

                if (existingAccount != null)
                {
                    ModelState.AddModelError("email", "Account email already in use");
                    return BadRequest(ModelState);
                }

                var createdAccount = await _accountRepository.CreateAccount(newAccount);

                return CreatedAtAction(nameof(GetAccount),
                    new { accountid = createdAccount.AccountId }, createdAccount);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new Account record");
            }
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<Plan>> GetPlan(string name)
        {
            try
            {
                var result = await _accountRepository.GetPlan(name);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }
    }
}
