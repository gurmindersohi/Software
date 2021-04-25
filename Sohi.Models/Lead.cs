using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sohi.Models
{
    public class Lead : ModelBase
    {
        public Guid LeadId { get; set; }

        //[NotMapped]
        //public string EncryptedId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        public Guid AccountId { get; set; }
        public string LeadSource { get; set; }

        public Boolean IsPhoneCallAllowed { get; set; }
        public Boolean IsEmailAllowed { get; set; }
        public Boolean IsTextAllowed { get; set; }
        public Boolean IsMember { get; set; }

    }
}
