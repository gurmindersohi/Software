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

            string url = string.Format(endPoint + "/me/accounts?fields=id,name,picture&access_token={0}&limit=100", accesstoken);

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
            catch (Exception ex) {
                return ex.Message;
            }
        }

        public async Task<List<Post>> GetFacebookPosts(string pageid, string pagetoken, string endPoint)
        {
            List<Post> posts = new List<Post>();

            string url = string.Format(endPoint + "/{0}/posts?fields=id,full_picture,message,created_time,admin_creator&access_token={1}", pageid, pagetoken);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                foreach (var item in parsedobj["data"])
                {
                    Post post = new Post();

                    post.Id = item["id"].ToString();
                    if (item["full_picture"] != null)
                    {
                        post.Picture = item["full_picture"].ToString();
                    }

                    if (item["message"] != null)
                    {
                        post.Description = item["message"].ToString();
                    }

                    if (item["admin_creator"] != null)
                    {
                        string publishedBy = item["admin_creator"]["name"].ToString();
                        var time = Convert.ToDateTime(item["created_time"].ToString());
                        var createdBy = "Published by " + publishedBy + " on " + time.ToString("MMMM") + " " + time.Day.ToString() + ", " + time.Year.ToString() + " at " + time.ToString("hh") + ":" + time.Minute.ToString() + " " + time.ToString("tt"); ;
                        post.CreatedTime = createdBy;
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

    }
}
