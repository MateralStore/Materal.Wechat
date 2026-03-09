using Materal.MergeBlock.Wechat.DTO;
using Materal.Utils.AutoMapper;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Materal.MergeBlock.Wechat.AutoMapperProfiles;

/// <summary>
/// UserAccessToken映射配置
/// </summary>
public class UserAccessTokenProfile : Profile
{
    /// <summary>
    /// 构造方法
    /// </summary>
    public UserAccessTokenProfile()
    {
        CreateMap<SnsOAuth2AccessTokenResponse, UserAccessTokenDTO>((mapper, m, n) =>
        {
            n.UnionID = m.UnionId ?? string.Empty;
            n.OpenID = m.OpenId;
        });
    }
}
