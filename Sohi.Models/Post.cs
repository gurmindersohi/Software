using System;

namespace Sohi.Models
{
    public class Post
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public string Picture { get; set; }

        public string CreatedTime { get; set; }

        public string MediaType { get; set; }

        public PostInsights Insights { get; set; }

        public Profile Profile { get; set; }

    }
}
