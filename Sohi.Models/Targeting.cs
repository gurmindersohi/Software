using System;
namespace Sohi.Models
{
    public class Targeting
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string AudienceSize { get; set; }


        public string[] Path { get; set; }


        public string ParentPath { get; set; }

        public string ChildPath { get; set; }

        public string GrandChildPath { get; set; }

        public string GreatGrandChildPath { get; set; }

    }
}
