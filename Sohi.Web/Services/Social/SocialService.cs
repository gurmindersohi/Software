using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sohi.Models;
using System.Collections.Generic;

namespace Sohi.Web.Services.Social
{
    public class SocialService : ISocialService
    {
        private readonly HttpClient httpClient;


        [Inject]
        public IConfiguration configuration { get; set; }


        public SocialService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<SocialMedia> GetToken(string accountid, string platform)
        {

            SocialMedia account = new SocialMedia();

            var response = await httpClient.GetAsync($"api/Social/{accountid}/{platform}");


            //var response = await httpClient.GetJsonAsync<SocialMedia>($"api/Social/{accountid}/{platform}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                account.AccessToken = parsedobj["accessToken"].ToString();

                return account;
            }
            else
            {
                return null;
            }

        }

        public async Task DeleteAccount(Guid id)
        {
            await httpClient.DeleteAsync($"api/social/{id}");
        }

        public async Task<List<SocialMedia>> GetAllTokens(string accountid)
        {
            Guid id = new Guid(accountid);

            List<SocialMedia> accounts = new List<SocialMedia>();

            var response = await httpClient.GetAsync($"api/Social/{accountid}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JArray)JsonConvert.DeserializeObject(jsonResponse);

                foreach (var item in parsedobj)
                {

                    SocialMedia account = new SocialMedia();
                    account.Id = new Guid(item["id"].ToString());

                    account.PageId = item["pageId"].ToString();

                    account.Name = item["name"].ToString();

                    account.Image = item["image"].ToString();

                    account.Type = item["type"].ToString();
                    account.AccessToken = item["accessToken"].ToString();
                    account.Secret = item["secret"].ToString();
                    account.CreatedOn = DateTime.Parse(item["createdOn"].ToString());
                    account.TokenExpiryDate = DateTime.Parse(item["tokenExpiryDate"].ToString());
                    account.Email = item["email"].ToString();
                    account.UserId = item["userId"].ToString();
                    account.AccountId = item["accountId"].ToString();

                    accounts.Add(account);

                }
                return accounts;
            }
            else
            {
                return null;
            }

        }

        public async Task<SocialMedia> SaveToken(SocialMedia account)
        {
            return await httpClient.PostJsonAsync<SocialMedia>("api/Social", account);
        }

        public async Task<Profile> GetFacebookAccountAsync(string accesstoken, string endPoint)
        {
            try
            {
                Profile profile = new Profile();

                string url = string.Format(endPoint + "/me?fields=id,name,picture&access_token={0}", accesstoken);

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                    profile.Id = parsedobj["id"].ToString();
                    profile.Name = parsedobj["name"].ToString();
                    profile.Image = parsedobj["picture"]["data"]["url"].ToString();

                    return profile;

                }
                else
                {
                    return null;
                    //return response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public async Task<List<Profile>> GetFacebookPages(string accesstoken, string endPoint)
        {
            List<Profile> pages = new List<Profile>();

            string url = string.Format(endPoint + "/me/accounts?fields=id,name,picture,access_token&access_token={0}&limit=100", accesstoken);

            //string facebook_EndPoint = string.Format(FacebookAPIEndpoints.GetFacebookAccounts + "?access_token={0}&fields=id,name,about&limit=100", access_token);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                foreach (var item in parsedobj["data"])
                {
                    Profile page = new Profile();

                    page.Id = item["id"].ToString();
                    page.Name = item["name"].ToString();

                    if (item["picture"]["data"]["url"] != null)
                    {
                        page.Image = item["picture"]["data"]["url"].ToString();
                    }

                    page.Token = item["access_token"].ToString();

                    pages.Add(page);
                }

                return pages;
            }
            else
            {
                return null;
            }
        }


        public async Task<string> GenerateFacebookPageTokenAsync(string pageId, string userToken, string endPoint)
        {
            try
            {
                string url = string.Format(endPoint + "/{0}?fields=access_token&access_token={1}", pageId, userToken);

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);
                    string token = parsedobj["access_token"].ToString();
                    return token;

                }
                else
                {
                    return response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<List<Post>> GetFacebookPosts(string pageid, string pagetoken, string endPoint)
        {
            List<Post> posts = new List<Post>();


            string url = string.Format(endPoint + "/{0}?fields=id,name,picture,posts%7Bid,full_picture,message,created_time,admin_creator%7D&access_token={1}", pageid, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                //post.Profile = new Profile();

                //Post post = new Post();
                //post.Insights = new PostInsights();

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);


                var data = parsedobj["posts"]["data"];

                foreach (var item in data)
                {

                    Post post = new Post();
                    post.Insights = new PostInsights();
                    post.Profile = new Profile();

                    post.Profile.Id = parsedobj["id"].ToString();
                    post.Profile.Name = parsedobj["name"].ToString();
                    post.Profile.Image = parsedobj["picture"]["data"]["url"].ToString();

                    post.Id = item["id"].ToString();
                    if (item["full_picture"] != null)
                    {
                        post.Picture = item["full_picture"].ToString();
                    }

                    if (item["message"] != null)
                    {
                        post.Description = item["message"].ToString();
                    }

                    if (item["created_time"] != null)
                    {
                        var time = Convert.ToDateTime(item["created_time"].ToString());
                        var createdBy = time.ToString("MMMM") + " " + time.Day.ToString() + ", " + time.Year.ToString() + " at " + time.ToString("hh") + ":" + time.Minute.ToString() + " " + time.ToString("tt"); ;
                        post.CreatedTime = createdBy;
                    }

                    if (item["admin_creator"] != null)
                    {
                        string publishedBy = item["admin_creator"]["name"].ToString();
                        var time = Convert.ToDateTime(item["created_time"].ToString());
                        var createdBy = "Published by " + publishedBy + " on " + time.ToString("MMMM") + " " + time.Day.ToString() + ", " + time.Year.ToString() + " at " + time.ToString("hh") + ":" + time.ToString("mm") + " " + time.ToString("tt"); ;
                        post.CreatedTime = createdBy;
                    }


                    var result = await GetPostInsights(item["id"].ToString(), pagetoken, endPoint);

                    post.Insights.Post_reactions_like_total = result.Post_reactions_like_total.ToString();

                    post.Insights.Post_engaged_users = result.Post_engaged_users.ToString();

                    post.Insights.Post_impressions = result.Post_impressions.ToString();

                    post.Insights.Post_clicks = result.Post_clicks.ToString();

                    posts.Add(post);

                }

                return posts;
            }
            else
            {
                return null;
            }
        }


        public async Task<PostInsights> GetPostInsights(string postid, string pagetoken, string endPoint)
        {
            PostInsights postInsights = new PostInsights();
            string url = string.Format(endPoint + "/{0}?fields=insights.metric(post_reactions_like_total,post_engaged_users,post_impressions, post_clicks)&access_token={1}", postid, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                var data = parsedobj["insights"]["data"];

                postInsights.Id = postid;

                foreach (var item in data)
                {
                    if (item["name"].ToString() == "post_reactions_like_total")
                    {
                        postInsights.Post_reactions_like_total = item["values"][0]["value"].ToString();

                    }
                    if (item["name"].ToString() == "post_engaged_users")
                    {
                        postInsights.Post_engaged_users = item["values"][0]["value"].ToString();

                    }
                    if (item["name"].ToString() == "post_impressions")
                    {
                        postInsights.Post_impressions = item["values"][0]["value"].ToString();

                    }
                    if (item["name"].ToString() == "post_clicks")
                    {
                        postInsights.Post_clicks = item["values"][0]["value"].ToString();

                    }


                    //if (data[0]["name"].ToString() == "post_reactions_like_total")
                    //{
                    //    postInsights.Post_reactions_like_total = data[0]["values"][0]["value"].ToString();

                    //}
                    //if (data[1]["name"].ToString() == "Post_engaged_users")
                    //{
                    //    postInsights.Post_engaged_users = data[1]["values"][1]["value"].ToString();

                    //}
                    //if (data[2]["name"].ToString() == "Post_impressions")
                    //{
                    //    postInsights.Post_impressions = data[2]["values"][2]["value"].ToString();

                    //}
                    //if (data[3]["name"].ToString() == "Post_clicks")
                    //{
                    //    postInsights.Post_clicks = data[3]["values"][3]["value"].ToString();

                    //}

                }

                return postInsights;
            }
            else
            {
                return null;
            }
        }

        public async Task<Post> CreatePost(string PageId, string endPoint, FormUrlEncodedContent content)
        {

            //string url = string.Format(endPoint + "/{0}/photos", PageId);

            var response = await httpClient.PostAsync(endPoint, content);

            if (response.IsSuccessStatusCode)
            {


                return null;
            }
            else
            {
                return null;
            }
        }


        public async Task<List<Post>> GetFacebookScheduledPosts(string pageid, string pagetoken, string endPoint)
        {
            List<Post> posts = new List<Post>();


            string url = string.Format(endPoint + "/{0}?fields=id,name,picture,scheduled_posts%7Bid,full_picture,message,created_time,admin_creator,scheduled_publish_time%7D&access_token={1}", pageid, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);


                if (parsedobj["scheduled_posts"] != null)
                {
                    var data = parsedobj["scheduled_posts"]["data"];

                    foreach (var item in data)
                    {
                        Post post = new Post();

                        post.Profile = new Profile();

                        post.Profile.Id = parsedobj["id"].ToString();
                        post.Profile.Name = parsedobj["name"].ToString();
                        post.Profile.Image = parsedobj["picture"]["data"]["url"].ToString();

                        post.Id = item["id"].ToString();
                        if (item["full_picture"] != null)
                        {
                            post.Picture = item["full_picture"].ToString();
                        }

                        if (item["message"] != null)
                        {
                            post.Description = item["message"].ToString();
                        }

                        if (item["scheduled_publish_time"] != null)
                        {
                            var dateStamp = item["scheduled_publish_time"].ToString();

                            Double date = Double.Parse(dateStamp);

                            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(date).ToLocalTime();

                            //var time = Convert.ToDateTime(item["scheduled_publish_time"].ToString());
                            var createdBy = time.ToString("MMMM") + " " + time.Day.ToString() + ", " + time.Year.ToString() + " at " + time.ToString("hh") + ":" + time.ToString("mm") + " " + time.ToString("tt"); ;
                            post.CreatedTime = createdBy;
                        }



                        posts.Add(post);

                    }

                    return posts;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        public async Task<Profile> GetFacebookPage(string accesstoken, string endPoint)
        {
            Profile page = new Profile();

            string url = string.Format(endPoint + "/me?fields=id,name,picture&access_token={0}", accesstoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                page.Id = parsedobj["id"].ToString();
                page.Name = parsedobj["name"].ToString();

                if (parsedobj["picture"]["data"]["url"] != null)
                {
                    page.Image = parsedobj["picture"]["data"]["url"].ToString();
                }

                return page;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Profile>> GetInstagramAccounts(string accesstoken, string endPoint)
        {
            List<Profile> instagramAccounts = new List<Profile>();

            string url = string.Format(endPoint + "/me/instagram_accounts?fields=id,profile_pic,username&access_token={0}", accesstoken);


            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                foreach (var item in parsedobj["data"])
                {
                    Profile instagramAccount = new Profile();

                    instagramAccount.Id = item["id"].ToString();
                    instagramAccount.Name = item["username"].ToString();
                    instagramAccount.Image = item["profile_pic"].ToString();

                    instagramAccounts.Add(instagramAccount);
                }

                return instagramAccounts;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> LongLivedUserToken(string client_id, string client_secret, string endPoint, string shortLivedUserToken)
        {

            //string endPoint = configuration.GetSection("FacebookApp").GetSection("EndPoint").Value;
            //string client_id = configuration.GetSection("FacebookApp").GetSection("ClientId").Value;
            //string client_secret = configuration.GetSection("FacebookApp").GetSection("ClientSecret").Value;

            string longLivedUserToken = "";

            string grant_type = "fb_exchange_token";

            string url = string.Format(endPoint + "/oauth/access_token?grant_type={0}&client_id={1}&client_secret={2}&fb_exchange_token={3}", grant_type, client_id, client_secret, shortLivedUserToken);


            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                longLivedUserToken = parsedobj["access_token"].ToString();

                return longLivedUserToken;
            }
            else
            {
                return null;
            }
        }

        public async Task<Profile> GetInstagramAccountInfo(string pageId, string pagetoken, string endPoint)
        {
            Profile instagramAccount = new Profile();

            string url = string.Format(endPoint + "/{0}?fields=instagram_business_account%7Bid,username,profile_picture_url%7D&access_token={1}", pageId, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                if (parsedobj["instagram_business_account"] != null)
                {
                    instagramAccount.Id = parsedobj["instagram_business_account"]["id"].ToString();
                    instagramAccount.Name = parsedobj["instagram_business_account"]["username"].ToString();
                    instagramAccount.Image = parsedobj["instagram_business_account"]["profile_picture_url"].ToString();

                    return instagramAccount;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }

        public async Task<Profile> GetInstagramBusinessAccountDetails(string accountId, string pagetoken, string endPoint)
        {
            Profile instagramAccount = new Profile();

            string url = string.Format(endPoint + "/{0}?fields=id,name,profile_picture_url&access_token={1}", accountId, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                if (parsedobj != null)
                {
                    instagramAccount.Id = parsedobj["id"].ToString();
                    instagramAccount.Name = parsedobj["name"].ToString();
                    instagramAccount.Image = parsedobj["profile_picture_url"].ToString();

                    return instagramAccount;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }

        public async Task<string> GenerateFacebookTokenAsync(string client_id, string client_secret, string endPoint, string redirectUrl, string code)
        {

            string url = string.Format(endPoint + "/oauth/access_token?");

            var values = new Dictionary<string, string>{
                  { "client_id", client_id },
                  { "client_secret", client_secret },
                  { "redirect_uri", redirectUrl },
                  { "code", code },
                };

            var content = new FormUrlEncodedContent(values);

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);
                string token = parsedobj["access_token"].ToString();
                return token;

            }
            else
            {
                return response.ReasonPhrase;
            }
        }

        public async Task<List<Post>> GetInstagramMedia(string accountId, string pagetoken, string endPoint)
        {
            List<Post> posts = new List<Post>();

            string url = string.Format(endPoint + "/{0}?fields=id,name,profile_picture_url,media%7Bid,caption,media_product_type,media_url%7D&access_token={1}", accountId, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);


                if (parsedobj != null)
                {
                    var data = parsedobj["media"]["data"];

                    foreach (var item in data)
                    {
                        Post post = new Post();
                        post.Profile = new Profile();

                        post.Insights = new PostInsights();

                        post.Profile.Id = parsedobj["id"].ToString();
                        post.Profile.Name = parsedobj["name"].ToString();
                        post.Profile.Image = parsedobj["profile_picture_url"].ToString();



                        post.Id = item["id"].ToString();

                        if (item["media_product_type"] != null)
                        {
                            post.MediaType = item["media_product_type"].ToString();
                        }
                        if (item["caption"] != null)
                        {
                            post.Description = item["caption"].ToString();
                        }
                        if (item["media_url"] != null)
                        {
                            post.Picture = item["media_url"].ToString();
                        }

                        if (item["username"] != null)
                        {
                            post.CreatedTime = item["username"].ToString();
                        }


                        var result = await GetInstagramMediaInsights(item["id"].ToString(), pagetoken, endPoint);

                        if (result != null)
                        {
                            post.Insights.Post_reactions_like_total = result.Post_reactions_like_total.ToString();

                            post.Insights.Post_engaged_users = result.Post_engaged_users.ToString();

                            post.Insights.Post_impressions = result.Post_impressions.ToString();

                            post.Insights.Post_clicks = result.Post_clicks.ToString();

                        }

                        posts.Add(post);


                    }


                    return posts;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }

        public async Task<PostInsights> GetInstagramMediaInsights(string postid, string token, string endPoint)
        {
            PostInsights postInsights = new PostInsights();
            string url = string.Format(endPoint + "/{0}?fields=like_count,insights.metric(engagement,reach,impressions)&access_token={1}", postid, token);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                var data = parsedobj["insights"]["data"];

                postInsights.Id = postid;

                if (parsedobj["like_count"].ToString() != null)
                {
                    postInsights.Post_reactions_like_total = parsedobj["like_count"].ToString();

                }

                foreach (var item in data)
                {

                    if (item["name"].ToString() == "engagement")
                    {
                        postInsights.Post_engaged_users = item["values"][0]["value"].ToString();

                    }
                    if (item["name"].ToString() == "impressions")
                    {
                        postInsights.Post_impressions = item["values"][0]["value"].ToString();

                    }
                    if (item["name"].ToString() == "reach")
                    {
                        postInsights.Post_clicks = item["values"][0]["value"].ToString();

                    }

                }

                return postInsights;
            }
            else
            {
                return null;
            }
        }


        public async Task<string> CreateInstagramPostContainer(string PageId, string endPoint, FormUrlEncodedContent content)
        {

            //string url = string.Format(endPoint + "/{0}/media_publish?creation_id={1}&access_token={2}", PageId, containerId, token);

            string url = string.Format(endPoint + "/{0}/media", PageId);

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                var containerId = parsedobj["id"];

                return containerId.ToString();
            }
            else
            {
                return null;
            }
        }
        public async Task<string> CreateInstagramPost(string PageId, string endPoint, FormUrlEncodedContent content)
        {

            //string url = string.Format(endPoint + "/{0}/media_publish?creation_id={1}&access_token={2}", PageId, containerId, token);

            string url = string.Format(endPoint + "/{0}/media_publish", PageId);

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                var data = parsedobj["id"];

                return data.ToString();
            }
            else
            {
                return null;
            }
        }

        //Analytics

        public async Task<List<PageInsights>> GetPageInsights(string pageid, string pagetoken, string endPoint, string datePreset)
        {
            List<PageInsights> pageInsights = new List<PageInsights>();


            string url = string.Format(endPoint + "/{0}/insights?metric=page_total_actions,page_views_total,page_actions_post_reactions_like_total&date_preset={1}&period=day&access_token={2}", pageid, datePreset, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                var data = parsedobj["data"];


                foreach (var item in data)
                {

                    PageInsights pageInsight = new PageInsights();
                    List<Values> values = new List<Values>();

                    pageInsight.Id = item["id"].ToString();
                    pageInsight.Name = item["name"].ToString();
                    pageInsight.Title = item["title"].ToString();
                    pageInsight.Description = item["description"].ToString();
                    pageInsight.Period = item["period"].ToString();

                    int totalValue = 0;

                    foreach (var val in item["values"])
                    {
                        Values value = new Values();

                        value.Value = val["value"].ToString();

                        value.EndTime = Convert.ToDateTime(val["end_time"]);
                        totalValue += Convert.ToInt32(val["value"]);

                        values.Add(value);
                    }

                    pageInsight.TotalValue = totalValue;
                    pageInsight.Values = values;

                    pageInsights.Add(pageInsight);
                }

                return pageInsights;
            }
            else
            {
                return null;
            }
        }


    }
}
