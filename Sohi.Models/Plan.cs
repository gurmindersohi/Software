using System;
namespace Sohi.Models
{
    public class Plan
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public Decimal Price { get; set; }

        public string BillingPeriod { get; set; }

        public string ProductId { get; set; }

        public Decimal Tax { get; set; }

        public Decimal Total { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

    }
}
