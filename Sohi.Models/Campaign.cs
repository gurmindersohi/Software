using System;
using System.ComponentModel.DataAnnotations;

namespace Sohi.Models
{
    public class Campaign
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Objective { get; set; } = "LINK_CLICKS";

        public bool Status { get; set; }

        public string StatusString { get; set; }

        public DateTime StartTime { get; set; }


    }
}
