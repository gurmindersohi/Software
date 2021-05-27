using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public List<FacebookLocation> facebookLocation { get; set; }

        //protected async Task SearchLocation()
        //{

        //    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

        //    var result = await AdAccountService.SearchLocation(AccountId, endPoint, Pr);

        //    if (result != null)
        //    {
        //        facebookLocation = result;
        //    }


        //}



        protected async Task HandleValidSubmit()
        {

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("name", Adset.Name));
            values.Add(new KeyValuePair<string, string>("optimization_goal", Adset.OptimizationGoal));
            values.Add(new KeyValuePair<string, string>("billing_event", Adset.BillingEvent));
            values.Add(new KeyValuePair<string, string>("bid_amount", Adset.CostControl.ToString()));
            values.Add(new KeyValuePair<string, string>(Adset.Budget, Adset.DailyBudget.ToString()));
            values.Add(new KeyValuePair<string, string>("campaign_id", "23847554963840538"));
            values.Add(new KeyValuePair<string, string>("status", "PAUSED"));
            values.Add(new KeyValuePair<string, string>("access_token", Profile.Token));
            values.Add(new KeyValuePair<string, string>("genders", Adset.Gender));
            values.Add(new KeyValuePair<string, string>("age_min", Adset.MinAge.ToString()));
            values.Add(new KeyValuePair<string, string>("age_max", Adset.MaxAge.ToString()));



            Dictionary<string, object> targeting = new Dictionary<string, object>();

            Dictionary<string, string[]> geoLocation = new Dictionary<string, string[]>();


            ////Dictionary<string, string[]> countries = new Dictionary<string, string[]>();

            //string[] countries = { "US" };

            ////countries.Add("countries", countries);


            //Dictionary<string, string[]> city = new Dictionary<string, string[]>();

            //string[] cities = { "Brampton" };

            //city.Add("city", cities);


            string[] countries = { "US", "CA" };
            string[] cities = { "Brampton" };

            geoLocation.Add("countries", countries);
            geoLocation.Add("city", cities);

            targeting.Add("geo_locations", geoLocation);


            string[] platforms = { "facebook" , "audience_network" };

            targeting.Add("publisher_platforms", platforms);


            var targets = targeting.ToString();

            var result = GeoLocation(geoLocation);


            values.Add(new KeyValuePair<string, string>("targeting", targets));



            var content = new FormUrlEncodedContent(values);


            //var content = new FormUrlEncodedContent(new[]
            //               {
            //                    new KeyValuePair<string, string>("name", Adset.Name),
            //                    new KeyValuePair<string, string>("optimization_goal", Adset.OptimizationGoal),
            //                    new KeyValuePair<string, string>("billing_event", Adset.BillingEvent),
            //                    new KeyValuePair<string, string>("bid_amount", Adset.CostControl.ToString()),
            //                    new KeyValuePair<string, string>(Adset.Budget, Adset.DailyBudget.ToString()),
            //                    new KeyValuePair<string, string>("campaign_id", "23847554963840538"),
            //                    new KeyValuePair<string, string>("status", "PAUSED"),
            //                    new KeyValuePair<string, string>("access_token", Profile.Token),
            //                    new KeyValuePair<string, string>("genders", Adset.Gender),
            //                    new KeyValuePair<string, string>("age_min", Adset.MinAge.ToString()),
            //                    new KeyValuePair<string, string>("age_max", Adset.MaxAge.ToString()),


            //                });




            var f = content;

            //CampaignId = await AdAccountService.CreateFacebookCampaign(AccountId, endPoint, content);


            //if (CampaignId != null)
            //{
            //    NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Adsets/" + CampaignId);
            //}

        }


        public string GeoLocation(Dictionary<string, string[]> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, string[]> keyValues in dictionary)
            {
                var str = String.Join(",", "'" + keyValues.Value + "'");

                dictionaryString += "'" + keyValues.Key + "'" + ":" + "[" +  str + "],";
            }

            return dictionaryString.TrimEnd(',', ' ') + "}";

            //String.Format("'geo_locations':{'cities':[{'key':'" + id + "','radius':'" + DropDownList2.SelectedValue + "','distance_unit':'mile'}]}");

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
