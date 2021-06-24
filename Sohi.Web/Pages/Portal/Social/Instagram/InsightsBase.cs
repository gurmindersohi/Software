using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social.Instagram
{
    public class InsightsBase : ComponentBase
    {

        [Parameter]
        public string PageId { get; set; }

        private string EndPoint { get; set; }

        protected string InsightsType { get; set; } = "Impressions";

        protected string Period { get; set; } = "last_7d";

        protected DateTime Since { get; set; } = DateTime.Now.AddDays(-28);

        protected DateTime Until { get; set; } = DateTime.Now;


        public List<PageInsights> PageInsights { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        public bool flag { get; set; } = false;

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [CascadingParameter(Name = "SocialProfile")]
        public Profile Profile { get; set; }

        [CascadingParameter(Name = "CurrentUser")]
        public User user { get; set; }


        public PageInsights Impressions { get; set; }

        public PageInsights Reach { get; set; }

        public PageInsights ProfileViews { get; set; }


        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;


        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                string pageid = Profile.Id;

                string PageToken = Profile.Token;

                string data_Preset = Period;


                if (PageToken != null)
                {

                    string since = ((DateTimeOffset)Since).ToUnixTimeSeconds().ToString();

                    string until = ((DateTimeOffset)Until).ToUnixTimeSeconds().ToString();

                    await GetInstagramInsights(pageid, PageToken, since, until);


                    //var result = await SocialService.GetPageInsights(pageid, PageToken, EndPoint, data_Preset);

                }
            }
        }

        protected async Task OnSinceChanged(ChangeEventArgs e)
        {
            Since = Convert.ToDateTime(e.Value);

            await OnPeriodChange();
        }

        protected async Task OnUntilChanged(ChangeEventArgs e)
        {
            Until = Convert.ToDateTime(e.Value);

            await OnPeriodChange();
        }

        protected async Task OnPeriodChange()
        {
            Impressions = null;
            Reach = null;
            ProfileViews = null;

            string since = ((DateTimeOffset)Since).ToUnixTimeSeconds().ToString();

            string until = ((DateTimeOffset)Until).ToUnixTimeSeconds().ToString();

            await GetInstagramInsights(Profile.Id, Profile.Token, since, until);

        }

        protected async Task OnInsightsType(ChangeEventArgs e)
        {
            InsightsType = e.Value.ToString();

            await GetInsights(InsightsType);
        }

        protected async Task GetInsights(string type)
        {

            if (InsightsType == "Impressions")
            {
                await ShowChart(Impressions, "Number of Impressions");
            }

            if (InsightsType == "Reach")
            {
                await ShowChart(Reach, "Number of Reach");
            }

            if (InsightsType == "Profile_Views")
            {
                await ShowChart(ProfileViews, "Number of Profile Views");
            }


        }

        private async Task GetInstagramInsights(string pageid, string PageToken, string since, string until)
        {
            var result = await SocialService.GetInstagramInsights(pageid, PageToken, EndPoint, since, until);
            if (result != null)
            {
                PageInsights = result;
                foreach (var item in result)
                {
                    if (item.Name == "impressions")
                    {
                        Impressions = item;
                    }
                    if (item.Name == "reach")
                    {
                        Reach = item;
                    }
                    if (item.Name == "profile_views")
                    {
                        ProfileViews = item;
                    }
                }

                await GetInsights(InsightsType);


                //flag = true;
            }
            else
            {
                flag = true;
            }
        }

        protected async Task ShowChart(PageInsights insights, string label)
        {
            List<string> data = new List<string>();
            List<string> labels = new List<string>();

            string headerLabel = label;

            foreach (var item in insights.Values)
            {
                data.Add(item.Value);
                labels.Add(item.EndTime.ToString("MMMM dd"));

            }

            var response = await JSRuntime.InvokeAsync<string>(identifier: "DrawChart", data.ToArray(), labels.ToArray(), headerLabel);

        }
    }
}
