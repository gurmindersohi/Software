using System;
using System.ComponentModel.DataAnnotations;

namespace Sohi.Models
{
    public class Adset
    {
        public string Id { get; set; }

        //[Required]
        public string Name { get; set; }

        //[Required]
        public string OptimizationGoal { get; set; } = "REACH";

        //[Required]
        public string BillingEvent { get; set; }

        //[Required]
        public string BidAmount { get; set; }

        //[Required]
        public string Budget { get; set; } = "daily_budget";

        //[Required]
        [Range(1.25, 1000000000000)]
        public decimal DailyBudget { get; set; }

        //[Required]
        public string CampaignId { get; set; }

        //[Required]
        public string Targeting { get; set; }

        //[Required]
        public string Status { get; set; }

        //[Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        //[Required]
        public DateTime StartTime { get; set; }

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        public DateTime EndTime { get; set; }


        //[Required]
        public int MinAge { get; set; } = 18;

        //[Required]
        public int MaxAge { get; set; } = 65;


        //[Required]
        public string Gender { get; set; } = "All";

        //[Required]
        public string Placements { get; set; } = "Auto";

        //[Required]
        public string LocationType { get; set; }


        //[Required]
        public string Location { get; set; }

        public decimal BillingEvents { get; set; }

        public decimal CostControl { get; set; }


    }
}
