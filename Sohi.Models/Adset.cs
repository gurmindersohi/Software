using System;
using System.ComponentModel.DataAnnotations;

namespace Sohi.Models
{
    public class Adset
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string OptimizationGoal { get; set; }

        [Required]
        public string BillingEvent { get; set; }

        [Required]
        public string BidAmount { get; set; }

        [Required]
        public string Budget { get; set; }

        [Required]
        public Int32 DailyBudget { get; set; }

        [Required]
        public string CampaignId { get; set; }

        [Required]
        public string Targeting { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Timestamp]
        public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);


    }
}
