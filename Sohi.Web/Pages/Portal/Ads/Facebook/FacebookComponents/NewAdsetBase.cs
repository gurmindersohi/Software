using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Ads;

namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class NewAdsetBase : ComponentBase
    {

        [Parameter]
        public string AccountId { get; set; }

        [Parameter]
        public string CampaignId { get; set; }

        public Adset Adset { get; set; } = new Adset();

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        //public bool ScheduleEndDate { get; set; } = false;

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        //public List<FacebookLocation> facebookLocation { get; set; }

        public List<FacebookLocation> SelectedCountries { get; set; } = new List<FacebookLocation>();

        public List<FacebookLocation> SelectedCities { get; set; } = new List<FacebookLocation>();

        public List<FacebookLocation> SelectedRegions { get; set; } = new List<FacebookLocation>();

        public List<FacebookLocation> SelectedAddress { get; set; } = new List<FacebookLocation>();


        //protected async Task SearchLocation()
        //{

        //    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

        //    var result = await AdAccountService.SearchLocation(AccountId, endPoint, Pr);

        //    if (result != null)
        //    {
        //        facebookLocation = result;
        //    }


        //}


        protected void LocationSelectionChanged(FacebookLocation facebookLocation)
        {

            if (facebookLocation != null)
            {
                if (facebookLocation.Type == "country")
                {
                    SelectedCountries.Add(facebookLocation);
                }
                else if (facebookLocation.Type == "city")
                {
                    SelectedCities.Add(facebookLocation);
                }
                else if (facebookLocation.Type == "region")
                {
                    SelectedRegions.Add(facebookLocation);
                }
                else if (facebookLocation.Type == "custom_location")
                {
                    SelectedAddress.Add(facebookLocation);
                }
            }
        }

        protected async Task HandleValidSubmit()
        {

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            object Targets = GetTargets();

            string time = "0";

            if (Adset.ScheduleEndDate) {

                time = Adset.EndDate.Date.ToString("yyyy-MM-dd") + " " + Adset.EndTime.ToString("HH:mm:ss") + " " + Adset.TimeZone;

            }

            string bid = null;
            string bidStrategy = null;

            if (Adset.CostControl < 1)
            {
                bid = null;
                bidStrategy = "LOWEST_COST_WITHOUT_CAP";
            }
            else
            {
                bidStrategy = null;
                bid = (Adset.CostControl * 100).ToString();
            }

            var content = new
            {
                name = Adset.Name,
                optimization_goal = Adset.OptimizationGoal,
                destination_type = Adset.destination_type,
                billing_event = Adset.BillingEvents,
                campaign_id = "23847569735650538",
                status = "PAUSED",
                access_token = Profile.Token,
                targeting = Targets,
                daily_budget = (Adset.DailyBudget * 100).ToString(),
                bid_amount = bid,
                bid_strategy = bidStrategy,
                start_time = Adset.StartDate.Date.ToString("yyyy-MM-dd") + " " + Adset.StartTime.ToString("HH:mm:ss") + " " + Adset.TimeZone,
                end_time = time
            };

            //var content = new
            //{
            //    name = "Test",
            //    optimization_goal = "LINK_CLICKS",
            //    destination_type = "WEBSITE",
            //    billing_event = "IMPRESSIONS",
            //    campaign_id = "23847569735650538",
            //    status = "PAUSED",
            //    access_token = Profile.Token,
            //    targeting = Targets,
            //    daily_budget = "1000",
            //    bid_strategy = "LOWEST_COST_WITHOUT_CAP",
            //    start_time = "2021-05-30 00:00:00 PDT",
            //    end_time = 0


            //};




            // Post Request

            var adsetId = await AdAccountService.CreateFacebookAdSet(AccountId, endPoint, content);


            if (adsetId != null)
            {
                NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Ads/" + adsetId);
            }

        }



        private object GetTargets()
        {
            List<object> Countries = new List<object>();

            foreach (var item in SelectedCountries)
            {
                Countries.Add(item.Key);
            }


            List<object> Regions = new List<object>();

            foreach (var item in SelectedRegions)
            {
                var region = new
                {
                    key = item.Key,
                };

                Regions.Add(region);
            }


            List<object> Cities = new List<object>();

            foreach (var item in SelectedCities)
            {
                var city = new
                {
                    key = item.Key,
                    radius = "10",
                    distance_unit = "mile"
                };

                Cities.Add(city);
            }


            List<object> CustomLocations = new List<object>();

            foreach (var item in SelectedAddress)
            {
                var address = new
                {
                    address_string = item.Key,
                    radius = "10",
                    distance_unit = "mile"
                };

                CustomLocations.Add(address);
            }


            var geoLocations = new
            {
                countries = Countries,
                regions = Regions,
                cities = Cities,
                custom_locations = CustomLocations
            };



            string[] gender = null;

            if (Adset.Gender != "All")
            {
                gender.Append(Adset.Gender);
            }



            var targets = new
            {
                age_min = Adset.MinAge.ToString(),
                age_max = Adset.MaxAge.ToString(),

                genders = gender,

                geo_locations = geoLocations
            };

            return targets;
        }


        //protected async Task HandleValidSubmit()
        //{

        //    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

        //    var values = new List<KeyValuePair<string, string>>();

        //    values.Add(new KeyValuePair<string, string>("name", Adset.Name));
        //    values.Add(new KeyValuePair<string, string>("optimization_goal", Adset.OptimizationGoal));
        //    values.Add(new KeyValuePair<string, string>("billing_event", Adset.BillingEvents));
        //    values.Add(new KeyValuePair<string, string>(Adset.Budget, (Adset.DailyBudget * 100).ToString()));
        //    values.Add(new KeyValuePair<string, string>("campaign_id", "23847554963840538"));
        //    values.Add(new KeyValuePair<string, string>("status", "PAUSED"));
        //    values.Add(new KeyValuePair<string, string>("access_token", Profile.Token));


        //    var startDate = Adset.StartDate;
        //    var startTime = Adset.StartTime;

        //    var start_time = "";

        //    values.Add(new KeyValuePair<string, string>("start_time", start_time));

        //    if (Adset.CostControl != 0)
        //    {
        //        values.Add(new KeyValuePair<string, string>("bid_amount", (Adset.CostControl * 100).ToString()));
        //    }
        //    else
        //    {
        //        values.Add(new KeyValuePair<string, string>("bid_strategy", "LOWEST_COST_WITHOUT_CAP"));
        //    }


        //    string geolocation = GetGeoLocation(SelectedCountries, SelectedCities, SelectedRegions);


        //    var targeting = string.Format("{0}'age_min':'{1}','age_max':'{2}','geo_locations':{3}{4}", "{", Adset.MinAge.ToString(),
        //        Adset.MaxAge.ToString(), geolocation, "}");

        //    values.Add(new KeyValuePair<string, string>("targeting", targeting));

        //    var content = new FormUrlEncodedContent(values);



        //    var adsetId = await AdAccountService.CreateFacebookAdSet(AccountId, endPoint, content);


        //    //if (adsetId != null)
        //    //{
        //    //    NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Ads/" + adsetId);
        //    //}

        //}



    }
}
