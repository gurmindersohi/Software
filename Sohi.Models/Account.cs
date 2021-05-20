using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sohi.Models
{
    public class Account : ModelBase
    {
		public Guid AccountId { get; set; }

		public string AccountName { get; set; }
		public string AccountType { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string Province { get; set; }
		public string Country { get; set; }
		public string PostalCode { get; set; }
		public string UsersLimit { get; set; }

		public string Logo { get; set; }
		public DateTime TrialExpiry { get; set; }
		public Boolean IsAccountPaid { get; set; }
		public Boolean IsDeleted { get; set; }
		public Boolean OnHold { get; set; }

		public DateTime HoldDate { get; set; }
		
	}
}
