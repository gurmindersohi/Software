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

        public bool ScheduleEndDate { get; set; } = false;

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
            }

            //foreach (var fbLocations in facebookLocations)
            //{
            //    if (fbLocations.Type == "country")
            //    {
            //        SelectedCountries.Add(fbLocations);
            //    }
            //}

            //SelectedLocation = facebookLocations;
        }

        protected async Task TestFunction()
        {

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;


            //string[] country = { "CA", "US" };
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


            var geoLocations = new
            {
                countries = Countries,
                regions = Regions,
                cities = Cities
            };


            var targets = new
            {
                age_min = "18",
                age_max = "64",

                geo_locations = geoLocations
            };


            var content = new
            {
                name = Adset.Name,
                optimization_goal = "IMPRESSIONS",
                billing_event = "IMPRESSIONS",
                campaign_id = "23847569735650538",
                status = "PAUSED",
                access_token = Profile.Token,
                targeting = targets,
                daily_budget = "1000",
                bid_strategy = "LOWEST_COST_WITHOUT_CAP"


            };


            var adsetId = await AdAccountService.NewCreateFacebookAdSet(AccountId, endPoint, content);


            var body = content;

            //using var response = await HttpClient.PostJsonAsync("https://reqres.in/invalid-url", postBody);

        }

        protected async Task HandleValidSubmit()
        {

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("name", Adset.Name));
            values.Add(new KeyValuePair<string, string>("optimization_goal", Adset.OptimizationGoal));
            values.Add(new KeyValuePair<string, string>("billing_event", Adset.BillingEvents));
            values.Add(new KeyValuePair<string, string>(Adset.Budget, (Adset.DailyBudget * 100).ToString()));
            values.Add(new KeyValuePair<string, string>("campaign_id", "23847554963840538"));
            values.Add(new KeyValuePair<string, string>("status", "PAUSED"));
            values.Add(new KeyValuePair<string, string>("access_token", Profile.Token));


            var startDate = Adset.StartDate;
            var startTime = Adset.StartTime;

            var start_time = "";

            values.Add(new KeyValuePair<string, string>("start_time", start_time));

            if (Adset.CostControl != 0)
            {
                values.Add(new KeyValuePair<string, string>("bid_amount", (Adset.CostControl * 100).ToString()));
            }
            else
            {
                values.Add(new KeyValuePair<string, string>("bid_strategy", "LOWEST_COST_WITHOUT_CAP"));
            }


            string geolocation = GetGeoLocation(SelectedCountries, SelectedCities, SelectedRegions);


            var targeting = string.Format("{0}'age_min':'{1}','age_max':'{2}','geo_locations':{3}{4}", "{", Adset.MinAge.ToString(),
                Adset.MaxAge.ToString(), geolocation, "}");

            values.Add(new KeyValuePair<string, string>("targeting", targeting));

            var content = new FormUrlEncodedContent(values);



            var adsetId = await AdAccountService.CreateFacebookAdSet(AccountId, endPoint, content);


            //if (adsetId != null)
            //{
            //    NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Ads/" + adsetId);
            //}

        }


        private string GetGeoLocation(List<FacebookLocation> selectedCountries, List<FacebookLocation> selectedCities, List<FacebookLocation> selectedRegions)
        {

            Dictionary<string, List<FacebookLocation>> GeoLocationCountry = new Dictionary<string, List<FacebookLocation>>();
            GeoLocationCountry.Add("countries", selectedCountries);
            var AllCountries = GetCountryLocations(GeoLocationCountry);


            Dictionary<string, List<FacebookLocation>> GeoLocationRegion = new Dictionary<string, List<FacebookLocation>>();
            GeoLocationRegion.Add("regions", selectedRegions);
            var AllRegion = GetRegionLocations(GeoLocationRegion);


            Dictionary<string, List<FacebookLocation>> GeoLocationCity = new Dictionary<string, List<FacebookLocation>>();
            GeoLocationCity.Add("cities", selectedCities);
            var AllCities = GetCityLocations(GeoLocationCity);

            var result = AllCountries + "," + AllRegion + "," + AllCities;

            return result;

        }


        //public string GetGeoLocations(Dictionary<string, List<string>> dictionary)
        //{
        //    string dictionaryString = "{";
        //    foreach (KeyValuePair<string, List<string>> keyValues in dictionary)
        //    {
        //        string ws = string.Format("'{0}'", string.Join("','", keyValues.Value.Select(i => i.Replace("'", "''"))));
        //        dictionaryString += "'" + keyValues.Key + "'" + ":" + "[" + ws + "],";
        //    }

        //    return dictionaryString.TrimEnd(',', ' ') + "}";
        //}

        public string GetCountryLocations(Dictionary<string, List<FacebookLocation>> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, List<FacebookLocation>> keyValues in dictionary)
            {
                string ws = string.Format("'{0}'", string.Join("','", keyValues.Value.Select(i => i.Key.Replace("'", "''"))));
                dictionaryString += "'" + keyValues.Key + "'" + ":" + "[" + ws + "],";
            }

            return dictionaryString.TrimEnd(',', ' ') + "}";
        }

        public string GetRegionLocations(Dictionary<string, List<FacebookLocation>> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, List<FacebookLocation>> keyValues in dictionary)
            {
                string result = string.Join(",", keyValues.Value.Select(x => string.Format("{0}'{1}'{2}", "{'key':", x.Key, "}")));

                dictionaryString += "'" + keyValues.Key + "'" + ":" + "[" + result + "],";
            }

            return dictionaryString.TrimEnd(',', ' ') + "}";
        }

        public string GetCityLocations(Dictionary<string, List<FacebookLocation>> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, List<FacebookLocation>> keyValues in dictionary)
            {
                string result = string.Join(",", keyValues.Value
                    .Select(x => string.Format("{0}'{1}'{2}'{3}'{4}'{5}'{6}", "{'key':", x.Key, ",'radius':", x.Radius, ",'distance_unit':", x.DistanceUnit, "}")));

                dictionaryString += "'" + keyValues.Key + "'" + ":" + "[" + result + "],";
            }

            return dictionaryString.TrimEnd(',', ' ') + "}";
        }


        public string DictionaryToString(Dictionary<string, string[]> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, string[]> keyValues in dictionary)
            {
                dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";
            }
            return dictionaryString.TrimEnd(',', ' ') + "}";
        }



        //private string Targets()
        //{

        //    StringBuilder target = new StringBuilder();
        //    target.Append("{");
        //    if (ddlEndAgeGroup.SelectedValue == "65 +")
        //    {
        //        target.Append(string.Format("'age_min':{0},", ddlStartAgeGroup.SelectedValue));

        //    }
        //    else if (Convert.ToInt16(ddlEndAgeGroup.SelectedValue) <= 64)
        //    {

        //        target.Append(string.Format("'age_min':{0},'age_max':{1},", ddlStartAgeGroup.SelectedValue, ddlEndAgeGroup.SelectedValue));
        //    }

        //    // add your code here parth



        //    if (ddlCreateGender.SelectedValue == "All")
        //    {
        //        target.Append("'genders':[1,2],");
        //    }
        //    else
        //    {
        //        target.Append("'genders':[" + ddlCreateGender.SelectedValue + "],");
        //    }


        //    //by parth
        //    if (ddl_adset_custom_aud.SelectedIndex != 0)
        //    {
        //        target.Append("'custom_audiences':[{ 'id':" + ddl_adset_custom_aud.SelectedValue + "}],");
        //    }

        //    if (locAddress.Checked == true)
        //    {
        //        string address = hiddenadsetAddress.Value;
        //        target.Append("'geo_locations':{'custom_locations':[{'address_string':'" + address + "','radius':'" + DropDownList1.SelectedValue + "','distance_unit':'mile'}]}");

        //    }
        //    else if (locCity.Checked == true)
        //    {
        //        string type = hiddentype.Value;
        //        string id = hiddenCityorCountryId.Value;

        //        if (type == "city")
        //        {
        //            target.Append("'geo_locations':{'cities':[{'key':'" + id + "','radius':'" + DropDownList2.SelectedValue + "','distance_unit':'mile'}]}");
        //        }
        //        else if (type == "country")
        //        {
        //            target.Append("'geo_locations':{'countries':['" + id + "']}");
        //        }
        //        else if (type == "region")
        //        {
        //            target.Append("'geo_locations':{'regions':[{'key':'" + id + "'}]}");
        //        }
        //    }


        //    //// Placements

        //    if (rdnCreateAutoPalcements.Checked == true)
        //    {
        //        //target.Append("'device_platforms':['mobile','desktop']");
        //    }
        //    else if (rdnCreateEditPlacement.Checked == true)
        //    {
        //        List<string> facebookList = new List<string>();
        //        List<string> audienceList = new List<string>();
        //        List<string> instagramList = new List<string>();

        //        foreach (ListItem item in chklistFacebook.Items)
        //        {
        //            if (item.Selected)
        //            {
        //                facebookList.Add("'" + item.Value + "'");
        //            }
        //        }
        //        string facebook_positions = String.Join(",", facebookList);

        //        foreach (ListItem item in chklistAudienceNetwork.Items)
        //        {
        //            if (item.Selected)
        //            {
        //                audienceList.Add("'" + item.Value + "'");
        //            }
        //        }
        //        string audience_positions = String.Join(",", audienceList);

        //        foreach (ListItem item in chklistInstagram.Items)
        //        {
        //            if (item.Selected)
        //            {
        //                instagramList.Add("'" + item.Value + "'");
        //            }
        //        }
        //        string instagram_positions = String.Join(",", instagramList);



        //        if (facebook_positions != "" && audience_positions == "" && instagram_positions == "")
        //        {
        //            target.Append(",'publisher_platforms':['facebook']");
        //            target.Append(",'facebook_positions':[" + facebook_positions + "]");
        //        }
        //        else if (facebook_positions == "" && audience_positions != "" && instagram_positions == "")
        //        {
        //            target.Append(",'publisher_platforms':['audience_network']");
        //            target.Append(",'audience_network_positions':[" + audience_positions + "]");
        //        }
        //        else if (facebook_positions == "" && audience_positions == "" && instagram_positions != "")
        //        {
        //            target.Append(",'publisher_platforms':['instagram']");
        //            target.Append(",'instagram_positions':[" + instagram_positions + "]");
        //        }
        //        else if (facebook_positions != "" && audience_positions != "" && instagram_positions != "")
        //        {
        //            target.Append(",'publisher_platforms':['facebook','audience_network','instagram']");
        //            target.Append(",'facebook_positions':[" + facebook_positions + "]");
        //            target.Append(",'audience_network_positions':[" + audience_positions + "]");
        //            target.Append(",'instagram_positions':[" + instagram_positions + "]");
        //        }
        //    }



        //    if (Interests.Value == null || Interests.Value == "")
        //    {
        //        //target.Append("}");
        //    }
        //    else
        //    {
        //        string interestshiddenvalue = Interests.Value;

        //        string[] InterestsArray = interestshiddenvalue.Split(",".ToCharArray());

        //        List<string> InterestList = new List<string>();

        //        foreach (var item in InterestsArray)
        //        {
        //            InterestList.Add("{'id':" + item + ",'name':''}");

        //        }

        //        string intereststargets = String.Join(",", InterestList);
        //        string interests = ",'interests':[" + intereststargets + "]";

        //        target.Append(interests);

        //    }

        //    if (Behaviors.Value == null || Behaviors.Value == "")
        //    {
        //        //target.Append("}");
        //    }
        //    else
        //    {
        //        string behaviorshiddenvalue = Behaviors.Value;

        //        string[] BehaviorsArray = behaviorshiddenvalue.Split(",".ToCharArray());

        //        List<string> BehaviorsList = new List<string>();

        //        foreach (var item in BehaviorsArray)
        //        {
        //            BehaviorsList.Add("{'id':" + item + ",'name':''}");
        //        }

        //        string behaviorstargets = String.Join(",", BehaviorsList);

        //        string behaviors = ",'behaviors':[" + behaviorstargets + "]";

        //        target.Append(behaviors);

        //    }

        //    if (LifeEvents.Value == null || LifeEvents.Value == "")
        //    {
        //        //target.Append("}");
        //    }
        //    else
        //    {
        //        string LifeEventshiddenvalue = LifeEvents.Value;

        //        string[] LifeEventsArray = LifeEventshiddenvalue.Split(",".ToCharArray());

        //        List<string> LifeEventsList = new List<string>();

        //        foreach (var item in LifeEventsArray)
        //        {
        //            LifeEventsList.Add("{'id':" + item + ",'name':''}");
        //        }

        //        string LifeEventstargets = String.Join(",", LifeEventsList);

        //        string lifeevents = ",'life_events':[" + LifeEventstargets + "]";

        //        target.Append(LifeEvents);

        //    }

        //    if (Industries.Value == null || Industries.Value == "")
        //    {
        //        //target.Append("}");
        //    }
        //    else
        //    {
        //        string Industrieshiddenvalue = Industries.Value;

        //        string[] IndustriesArray = Industrieshiddenvalue.Split(",".ToCharArray());

        //        List<string> IndustriesList = new List<string>();

        //        foreach (var item in IndustriesArray)
        //        {
        //            IndustriesList.Add("{'id':" + item + ",'name':''}");
        //        }

        //        string Industriestargets = String.Join(",", IndustriesList);

        //        string industries = ",'industries':[" + Industriestargets + "]";

        //        target.Append(industries);

        //    }

        //    if (Income.Value == null || Income.Value == "")
        //    {
        //        //target.Append("}");
        //    }
        //    else
        //    {
        //        string Incomehiddenvalue = Income.Value;

        //        string[] IncomeArray = Incomehiddenvalue.Split(",".ToCharArray());

        //        List<string> IncomeList = new List<string>();

        //        foreach (var item in IncomeArray)
        //        {
        //            IncomeList.Add("{'id':" + item + ",'name':''}");
        //        }

        //        string Incometargets = String.Join(",", IncomeList);

        //        string income = ",'income':[" + Incometargets + "]";

        //        target.Append(income);

        //    }

        //    if (FamilyStatuses.Value == null || FamilyStatuses.Value == "")
        //    {
        //        target.Append("}");
        //    }
        //    else
        //    {
        //        string FamilyStatuseshiddenvalue = FamilyStatuses.Value;

        //        string[] FamilyStatusesArray = FamilyStatuseshiddenvalue.Split(",".ToCharArray());

        //        List<string> FamilyStatusesList = new List<string>();

        //        foreach (var item in FamilyStatusesArray)
        //        {
        //            FamilyStatusesList.Add("{'id':" + item + ",'name':''}");
        //        }

        //        string FamilyStatusestargets = String.Join(",", FamilyStatusesList);

        //        string familystatuses = ",'family_statuses':[" + FamilyStatusestargets + "]}";

        //        target.Append(familystatuses);

        //    }

        //    //if (UserDevice.Value == null || UserDevice.Value == "")
        //    //{
        //    //    target.Append("}");
        //    //}
        //    //else
        //    //{
        //    //    string UserDevicehiddenvalue = UserDevice.Value;

        //    //    string[] UserDeviceArray = UserDevicehiddenvalue.Split(",".ToCharArray());

        //    //    List<string> UserDeviceList = new List<string>();

        //    //    foreach (var item in UserDeviceArray)
        //    //    {
        //    //        UserDeviceList.Add("{'id':" + item + ",'name':''}");
        //    //    }

        //    //    string UserDevicetargets = String.Join(",", UserDeviceList);

        //    //    string userdevice = ",'user_device':[" + UserDevicetargets + "]}";

        //    //    target.Append(userdevice);

        //    //}


        //    //

        //    //string[] gender = new string[2];

        //    //if (ddlCreateGender.SelectedValue == "All")
        //    //{
        //    //    gender[0] = "1";
        //    //    gender[1] = "2";
        //    //}
        //    //else
        //    //{
        //    //    gender[0] = ddlCreateGender.SelectedValue.ToString();
        //    //}

        //    return target.ToString();
        //}

    }
}
