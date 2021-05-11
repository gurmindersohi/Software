using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Api.Models.Social
{
    public interface ISocialRepository
    {

        Task<SocialMedia> SaveToken(SocialMedia account);

        Task<SocialMedia> GetTokenByPlatformAsync(string accountid, string platform);

        Task<IEnumerable<SocialMedia>> GetAllTokens(string accountid);

    }
}
