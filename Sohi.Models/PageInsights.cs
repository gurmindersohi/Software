using System;
using System.Collections.Generic;

namespace Sohi.Models
{
    public class PageInsights
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Period { get; set; }

        public int TotalValue { get; set; }

        public List<Values> Values { get; set; }
    }
}
