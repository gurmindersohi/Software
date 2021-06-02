using System;
namespace Sohi.Models
{
    public class Ad
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string AdSetup { get; set; } = "CreateAd";

        public string AdType { get; set; } = "Single";

        public string PrimaryText { get; set; }

        public string Destination { get; set; } = "Website";

        public string Headline { get; set; }

        public string WebsitrUrl { get; set; }

        public string Description { get; set; }

        public string DisplayLink { get; set; }

        public string CallToAction { get; set; } = "LEARN_MORE";
    }
}
