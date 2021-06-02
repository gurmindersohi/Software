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
    }
}
