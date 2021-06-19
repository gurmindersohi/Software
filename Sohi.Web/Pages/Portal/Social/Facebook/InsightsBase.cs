using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class InsightsBase : ComponentBase
    {
        private string EndPoint { get; set; }


        protected string InsightsType { get; set; } = "Page_Views";


        protected string Period { get; set; } = "last_7d";

        protected string SelectedDate { get; set; } = "June 3 - June 9";

        [Inject]
        public IConfiguration _config { get; set; }

        [Parameter]
        public string PageId { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        public List<PageInsights> PageInsights { get; set; }

        public PageInsights PageTotalActions { get; set; }

        public PageInsights PageTotalViews { get; set; }

        public PageInsights PageTotalLikes { get; set; }

        public bool flag { get; set; } = false;

        [Inject]
        public IJSRuntime JSRuntime { get; set; }


        [CascadingParameter(Name = "SocialProfile")]
        public Profile Profile { get; set; }

        [CascadingParameter(Name = "CurrentUser")]
        public User user { get; set; }

        protected override void OnParametersSet()
        {

        }

        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            string pageid = Profile.Id;

            string PageToken = Profile.Token;

            string data_Preset = Period;


            if (PageToken != null)
            {


                await GetPageInsights(pageid, PageToken, data_Preset);


                //var result = await SocialService.GetPageInsights(pageid, PageToken, EndPoint, data_Preset);

            }

        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    await ShowPageViewsChart();
        //}

        protected async Task OnPeriodChange(ChangeEventArgs e)
        {
            Period = e.Value.ToString();

            PageTotalActions = null;
            PageTotalViews = null;
            PageTotalLikes = null;

            //DateTime d2 = DateTime.Now.AddDays(-1);

            await GetPageInsights(Profile.Id, Profile.Token, Period);


        }

        private async Task GetPageInsights(string pageid, string PageToken, string data_Preset)
        {
            var result = await SocialService.GetPageInsights(pageid, PageToken, EndPoint, data_Preset);
            if (result != null)
            {
                PageInsights = result;
                foreach (var item in result)
                {
                    if (item.Name == "page_total_actions")
                    {
                        PageTotalActions = item;
                    }
                    if (item.Name == "page_views_total")
                    {
                        PageTotalViews = item;
                    }
                    if (item.Name == "page_actions_post_reactions_like_total")
                    {
                        PageTotalLikes = item;
                    }
                }

                await ShowPageViewsChart();

                //flag = true;
            }
            else
            {
                flag = true;
            }
        }

        protected async Task ShowPageViewsChart()
        {
            List<string> data = new List<string>();
            List<string> labels = new List<string>();

            foreach (var item in PageTotalViews.Values)
            {
                data.Add(item.Value);
                labels.Add(item.EndTime.ToString("MMMM dd"));

            }


            var response = await JSRuntime.InvokeAsync<string>(identifier: "AddData", data.ToArray(), labels.ToArray());

        }

    }
}
