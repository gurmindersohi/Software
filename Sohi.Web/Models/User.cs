using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sohi.Web.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public override string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public override string PasswordHash { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhotoPath { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string AccountId { get; set; }

        public string IsDeleted { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsActive { get; set; }

    }
}


