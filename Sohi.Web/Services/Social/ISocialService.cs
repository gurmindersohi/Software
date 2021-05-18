using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Web.Services.Social
{
    public interface ISocialService
    {
        Task<SocialMedia> SaveToken(SocialMedia account);

        Task<SocialMedia> GetToken(string accountid, string platform);

        Task<Profile> GetFacebookAccountAsync(string accesstoken, string endPoint);

        Task<List<Profile>> GetFacebookPages(string accesstoken, string endPoint);

        Task<string> GenerateFacebookPageTokenAsync(string pageid, string pagetoken, string endPoint);

        Task<List<Post>> GetFacebookPosts(string PageId, string PageToken, string endPoint);

        Task<Post> CreatePost(string PageId, string endPoint, FormUrlEncodedContent content);

        Task<List<SocialMedia>> GetAllTokens(string accountid);

        Task<List<Post>> GetFacebookScheduledPosts(string PageId, string PageToken, string endPoint);

        Task<Profile> GetFacebookPage(string pagetoken, string endPoint);

        Task<List<Profile>> GetInstagramAccounts(string accesstoken, string endPoint);

        Task<Profile> GetInstagramAccountInfo(string pageId, string pagetoken, string endPoint);

        Task<Profile> GetInstagramBusinessAccountDetails(string accountId, string pagetoken, string endPoint);


        Task<string> LongLivedUserToken(string client_id, string client_secret, string endPoint, string shortLivedUserToken);

        Task DeleteAccount(Guid id);

        Task<string> GenerateFacebookTokenAsync(string client_id, string client_secret, string endPoint, string redirectUrl, string code);

        Task<List<Post>> GetInstagramMedia(string accountId, string pagetoken, string endPoint);

        Task<string> CreateInstagramPostContainer(string PageId, string endPoint, FormUrlEncodedContent content);

        Task<string> CreateInstagramPost(string PageId, string endPoint, FormUrlEncodedContent content);

    }
}
